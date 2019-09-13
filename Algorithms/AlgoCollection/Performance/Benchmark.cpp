//
// Created by Zakhar on 16/03/2017.
//

#include <chrono>
#include <iostream>
#include <tuple>

#include "Benchmark.h"
#include <cassert>
#include "../Algebra/Auxiliary.h"

#include "../Algorithms/CDMissingValueRecovery.h"
#include "../Algorithms/TKCM.h"
#include "../Algorithms/ST_MVL.h"
#include "../Algorithms/SPIRIT.h"
#include "../Algorithms/GROUSE.h"
#include "../Algorithms/NMFMissingValueRecovery.h"
#include "../Algorithms/DynaMMo.h"
#include "../Algorithms/SVT.h"
#include "../Algorithms/ROSL.h"
#include "../Algorithms/IterativeSVD.h"
#include "../Algorithms/SoftImpute.h"
#include "../Algorithms/OGDImpute.h"
#include "../Algorithms/MD_ISVDAlgorithm.h"

using namespace Algorithms;

namespace Performance
{

void verifyRecovery(arma::mat &mat)
{
    for (uint64_t j = 0; j < mat.n_cols; ++j)
    {
        for (uint64_t i = 0; i < mat.n_rows; ++i)
        {
            if (std::isnan(mat.at(i, j)))
            {
                mat.at(i, j) = std::sqrt(std::numeric_limits<double>::max() / 100000.0);
            }
        }
    }
}

int64_t Recovery_CD(arma::mat &mat, uint64_t truncation)
{
    // Local
    int64_t result;
    CDMissingValueRecovery rmv(mat);
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;
    
    // Recovery
    rmv.setReduction(truncation);
    rmv.disableCaching = false;
    rmv.useNormalization = false;
    
    begin = std::chrono::steady_clock::now();
    rmv.autoDetectMissingBlocks();
    rmv.performRecovery(truncation == mat.n_cols);
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (CDRec): " << result << std::endl;
    
    verifyRecovery(mat);
    return result;
}

int64_t Recovery_TKCM(arma::mat &mat, uint64_t truncation)
{
    (void) truncation;
    
    // Local
    int64_t result;
    Algorithms::TKCM tkcm(mat);
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;

    // Recovery

    begin = std::chrono::steady_clock::now();
    tkcm.performRecovery();
    end = std::chrono::steady_clock::now();

    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (TKCM): " << result << std::endl;
    
    verifyRecovery(mat);
    return result;
}

int64_t Recovery_ST_MVL(arma::mat &mat, const std::string &latlong, uint64_t truncation)
{
    // Local
    int64_t result;
    ST_MVL stmvl(mat, latlong, 4.0, 0.85, truncation);
    
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;
    
    // Recovery
    
    begin = std::chrono::steady_clock::now();
    stmvl.Run(true);
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (STMVL): " << result << std::endl;
    
    verifyRecovery(mat);
    return result;
}

int64_t Recovery_SPIRIT(arma::mat &mat, uint64_t truncation)
{
    // Local
    int64_t result;
    
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;
    
    // Recovery
    begin = std::chrono::steady_clock::now();
    SPIRIT::doSpirit(mat, truncation, 6, 1.0);
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (SPIRIT): " << result << std::endl;
    
    verifyRecovery(mat);
    return result;
}

int64_t Recovery_GROUSE(arma::mat &mat, uint64_t truncation)
{
    // Local
    int64_t result;

    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;

    // Recovery

    mat = mat.t();
    
    GROUSE grouse(mat, truncation);

    begin = std::chrono::steady_clock::now();
    grouse.doGROUSE();
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (GROUSE): " << result << std::endl;
    
    mat = mat.t();
    
    verifyRecovery(mat);
    return result;
}

int64_t Recovery_NNMF(arma::mat &mat, uint64_t truncation)
{
    // Local
    int64_t result;
    
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;
    
    // Recovery
    begin = std::chrono::steady_clock::now();
    NMFMissingValueRecovery::doNMFRecovery(mat, truncation);
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (TeNMF): " << result << std::endl;
    
    verifyRecovery(mat);
    return result;
}

int64_t Recovery_DynaMMo(arma::mat &mat, uint64_t truncation)
{
    // Local
    int64_t result;
    
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;
    
    // Recovery
    
    mat = mat.t();
    
    begin = std::chrono::steady_clock::now();
    DynaMMo::doDynaMMo(mat, truncation, 100, true);
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (DynaMMo): " << result << std::endl;
    
    mat = mat.t();
    
    verifyRecovery(mat);
    return result;
}

int64_t Recovery_SVT(arma::mat &mat)
{
    // Local
    int64_t result;
    
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;
    
    // Recovery
    begin = std::chrono::steady_clock::now();
    SVT::doSVT(mat);
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (SVT): " << result << std::endl;
    
    verifyRecovery(mat);
    return result;
}

int64_t Recovery_ROSL(arma::mat &mat, uint64_t truncation)
{
    // Local
    int64_t result;
    
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;
    
    // Recovery
    begin = std::chrono::steady_clock::now();
    ROSL::ROSL_Recovery(mat, truncation, 0.6);
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (ROSL): " << result << std::endl;
    
    verifyRecovery(mat);
    return result;
}

int64_t Recovery_IterativeSVD(arma::mat &mat, uint64_t truncation)
{
    // Local
    int64_t result;
    
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;
    
    // Recovery
    begin = std::chrono::steady_clock::now();
    IterativeSVD::recoveryIterativeSVD(mat, truncation);
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (SVDImpute): " << result << std::endl;
    
    verifyRecovery(mat);
    return result;
}

int64_t Recovery_SoftImpute(arma::mat &mat, uint64_t truncation)
{
    // Local
    int64_t result;
    
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;
    
    // Recovery
    begin = std::chrono::steady_clock::now();
    SoftImpute::doSoftImpute(mat, truncation);
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (SoftImpute): " << result << std::endl;
    
    verifyRecovery(mat);
    return result;
}

int64_t Recovery_OGDImpute(arma::mat &mat, uint64_t truncation)
{
    // Local
    int64_t result;
    
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;
    
    OGDImpute ogd(mat, truncation);
    
    // Recovery
    begin = std::chrono::steady_clock::now();
    ogd.ARPredict();
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (OGDImp): " << result << std::endl;
    
    verifyRecovery(mat);
    return result;
}

int64_t Recovery_MDISVD(arma::mat &mat, uint64_t truncation)
{
    // Local
    int64_t result;
    
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;
    
    // Recovery
    
    mat = mat.t();
    
    MD_SVD isvd(mat, truncation);
    
    begin = std::chrono::steady_clock::now();
    isvd.doMDISVD();
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (MD-ISVD): " << result << std::endl;
    
    mat = mat.t();
    
    verifyRecovery(mat);
    return result;
}

// ================ streaming ==

int64_t Recovery_CD_Streaming(arma::mat &mat, uint64_t truncation)
{
    uint64_t streamStart = 0;
    
    for (uint64_t i = 0; i < mat.n_rows; ++i)
    {
        if (std::isnan(mat.at(i, 0)))
        {
            streamStart = i;
            break;
        }
    }
    
    uint64_t cutoff10 = mat.n_rows - (mat.n_rows / 10);
    streamStart = std::min(streamStart, cutoff10);
    
    arma::mat before_streaming = mat.submat(arma::span(0, streamStart - 1), arma::span::all);
    
    // Local
    int64_t result;
    CDMissingValueRecovery rmv(before_streaming);
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;
    
    // Recovery
    rmv.setReduction(truncation);
    rmv.disableCaching = false;
    rmv.useNormalization = false;
    
    rmv.autoDetectMissingBlocks();
    rmv.performRecovery(truncation == mat.n_cols);
    
    for (uint64_t i = streamStart; i < mat.n_rows; ++i)
    {
        rmv.increment(mat.row(i).t());
    }
    
    begin = std::chrono::steady_clock::now();
    rmv.autoDetectMissingBlocks();
    rmv.performRecovery(truncation == mat.n_cols);
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (InCD,stream): " << result << std::endl;
    
    mat = std::move(before_streaming);
    verifyRecovery(mat);
    return result;
}

int64_t Recovery_TKCM_Streaming(arma::mat &mat, uint64_t truncation)
{
    (void) truncation;
    
    // Local
    int64_t result;
    Algorithms::TKCM tkcm(mat);
    
    // Recovery
    result = tkcm.performRecovery(true);
    
    std::cout << "Time (TKCM,stream): " << result << std::endl;
    
    verifyRecovery(mat);
    return result;
}


int64_t Recovery_SPIRIT_Streaming(arma::mat &mat, uint64_t truncation)
{
    // Local
    int64_t result;
    
    // Recovery
    result = SPIRIT::doSpirit(mat, truncation, 6, 1.0);
    
    std::cout << "Time (SPIRIT,stream): " << result << std::endl;
    
    verifyRecovery(mat);
    return result;
}

int64_t Recovery_OGDImpute_Streaming(arma::mat &mat, uint64_t truncation)
{
    uint64_t streamStart = 0;
    
    for (uint64_t i = 0; i < mat.n_rows; ++i)
    {
        if (std::isnan(mat.at(i, 0)))
        {
            streamStart = i;
            break;
        }
    }
    
    uint64_t cutoff10 = mat.n_rows - (mat.n_rows / 10);
    streamStart = std::min(streamStart, cutoff10);
    
    arma::mat before_streaming = mat.submat(arma::span(0, streamStart - 1), arma::span::all);
    
    // Local
    int64_t result;
    OGDImpute ogd(before_streaming, truncation);
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;
    
    // Recovery
    ogd.ARPredict();
    
    for (uint64_t i = streamStart; i < mat.n_rows; ++i)
    {
        Algebra::Operations::increment_matrix(before_streaming, mat.row(i).t());
    }
    
    begin = std::chrono::steady_clock::now();
    ogd.ARPredict();
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (OGDImpute,stream): " << result << std::endl;
    
    mat = std::move(before_streaming);
    verifyRecovery(mat);
    return result;
}

int64_t Recovery_SAGE_Streaming(arma::mat &mat, uint64_t truncation)
{
    uint64_t streamStart = 0;
    
    for (uint64_t i = 0; i < mat.n_rows; ++i)
    {
        if (std::isnan(mat.at(i, 0)))
        {
            streamStart = i;
            break;
        }
    }// [!] despite transposing we search for the first index row-wise and later use it as column index
    
    uint64_t cutoff10 = mat.n_rows - (mat.n_rows / 10);
    streamStart = std::min(streamStart, cutoff10);
    
    mat = mat.t();
    
    arma::mat before_streaming = mat.submat(arma::span::all, arma::span(0, streamStart - 1));
    
    // Local
    int64_t result;
    GROUSE sage(before_streaming, truncation);
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;
    
    // Recovery
    sage.doGROUSE();
    
    begin = std::chrono::steady_clock::now();
    for (uint64_t i = streamStart; i < mat.n_cols; ++i)
    {
        Algebra::Operations::add_matrix_col(before_streaming, mat.col(i));
        sage.singleRowIncrementSAGE();
    }
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (SAGE,stream): " << result << std::endl;
    
    mat = std::move(before_streaming);
    verifyRecovery(mat);
    mat = mat.t();
    
    return result;
}

int64_t Recovery_MDISVD_Streaming(arma::mat &mat, uint64_t truncation)
{
    uint64_t streamStart = 0;
    
    for (uint64_t i = 0; i < mat.n_rows; ++i)
    {
        if (std::isnan(mat.at(i, 0)))
        {
            streamStart = i;
            break;
        }
    }// [!] despite transposing we search for the first index row-wise and later use it as column index
    
    uint64_t cutoff10 = mat.n_rows - (mat.n_rows / 10);
    streamStart = std::min(streamStart, cutoff10);
    
    mat = mat.t();
    
    arma::mat before_streaming = mat.submat(arma::span::all, arma::span(0, streamStart - 1));
    
    // Local
    int64_t result;
    MD_SVD isvd(before_streaming, truncation);
    std::chrono::steady_clock::time_point begin;
    std::chrono::steady_clock::time_point end;
    
    // Recovery
    isvd.doMDISVD();
    
    begin = std::chrono::steady_clock::now();
    for (uint64_t i = streamStart; i < mat.n_cols; ++i)
    {
        Algebra::Operations::add_matrix_col(before_streaming, mat.col(i));
    }
    isvd.doMDISVD();
    end = std::chrono::steady_clock::now();
    
    result = std::chrono::duration_cast<std::chrono::microseconds>(end - begin).count();
    std::cout << "Time (MD-ISVD,stream): " << result << std::endl;
    
    mat = std::move(before_streaming);
    verifyRecovery(mat);
    mat = mat.t();
    
    return result;
}

int64_t Recovery(arma::mat &mat, uint64_t truncation,
                 const std::string &algorithm, const std::string &xtra)
{
    if (xtra == "stream")
    {
        if (algorithm == "cd")
        {
            return Recovery_CD_Streaming(mat, truncation);
        }
        else if (algorithm == "tkcm")
        {
            return Recovery_TKCM_Streaming(mat, truncation);
        }
        else if (algorithm == "spirit")
        {
            return Recovery_SPIRIT_Streaming(mat, truncation);
        }
        else if (algorithm == "ogdimpute")
        {
            return Recovery_OGDImpute_Streaming(mat, truncation);
        }
        else if (algorithm == "grouse" || algorithm == "sage")
        {
            return Recovery_SAGE_Streaming(mat, truncation);
        }
        else if (algorithm == "mdisvd")
        {
            return Recovery_MDISVD_Streaming(mat, truncation);
        }
        else
        {
            std::cout << "Algorithm name '" << algorithm << "' does not exist or is not valid option for streaming" << std::endl;
            abort();
        }
    }
    
    if (algorithm == "cd")
    {
        return Recovery_CD(mat, truncation);
    }
    else if (algorithm == "tkcm")
    {
        return Recovery_TKCM(mat, truncation);
    }
    else if (algorithm == "st-mvl")
    {
        return Recovery_ST_MVL(mat, xtra, truncation);
    }
    else if (algorithm == "spirit")
    {
        return Recovery_SPIRIT(mat, truncation);
    }
    else if (algorithm == "grouse")
    {
        return Recovery_GROUSE(mat, truncation);
    }
    else if (algorithm == "nnmf")
    {
        return Recovery_NNMF(mat, truncation);
    }
    else if (algorithm == "dynammo")
    {
        return Recovery_DynaMMo(mat, truncation);
    }
    else if (algorithm == "svt")
    {
        return Recovery_SVT(mat);
    }
    else if (algorithm == "rosl")
    {
        return Recovery_ROSL(mat, truncation);
    }
    else if (algorithm == "itersvd")
    {
        return Recovery_IterativeSVD(mat, truncation);
    }
    else if (algorithm == "softimpute")
    {
        return Recovery_SoftImpute(mat, truncation);
    }
    else if (algorithm == "ogdimpute")
    {
        return Recovery_OGDImpute(mat, truncation);
    }
    else if (algorithm == "mdisvd")
    {
        return Recovery_MDISVD(mat, truncation);
    }
    else
    {
        std::cout << "Algorithm name '" << algorithm << "' is not valid" << std::endl;
        abort();
    }
}

} // namespace Performance
