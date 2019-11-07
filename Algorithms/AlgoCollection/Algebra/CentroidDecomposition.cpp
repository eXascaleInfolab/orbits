//
// Created by Zakhar on 08.03.2017.
//

#include <cmath>
#include <iostream>
#include "CentroidDecomposition.h"
#include "Auxiliary.h"

namespace Algorithms
{
//
// CentroidDecomposition constructors & desctructors
//

CentroidDecomposition::CentroidDecomposition(arma::mat &mx)
        : CentroidDecomposition(mx, mx.n_cols)
{ }

CentroidDecomposition::CentroidDecomposition(arma::mat &mx, uint64_t k)
        : Src(mx),
          Load(mx.n_rows, k),
          Rel(mx.n_cols, k),
          signVectors(std::vector<arma::vec>()),
          truncation(k),
          strategy(defaultSignVectorStrategy)
{
    Load.zeros();
    Rel.zeros();
    
    // i = 0:
    arma::vec signV(mx.n_rows);
    signV.fill(1.0);
    signVectors.emplace_back(std::move(signV));

    arma::vec direction(mx.n_cols);
    direction.fill(0.0);
    directions.emplace_back(std::move(direction));
    
    // i in [1,m[
    for (uint64_t i = 1; i < mx.n_cols; ++i)
    {
        signVectors.emplace_back(signVectors[0]);
        directions.emplace_back(directions[0]);
    }
}

//
// CentroidDecomposition API
//

const arma::mat &CentroidDecomposition::getLoad()
{
    return Load;
}

const arma::mat &CentroidDecomposition::getRel()
{
    return Rel;
}

arma::mat CentroidDecomposition::stealLoad()
{
    return std::move(Load);
}

arma::mat CentroidDecomposition::stealRel()
{
    return std::move(Rel);
}

void CentroidDecomposition::destroyDecomposition()
{
    Load.zeros();
    Rel.zeros();
}

void CentroidDecomposition::changeTruncation(uint64_t _k)
{
    truncation = _k;
    Load.resize(Load.n_rows, _k);
    Rel.resize(Rel.n_rows, _k);
}

void CentroidDecomposition::resetSignVectors()
{
    for (uint64_t i = 0; i < Src.n_cols; ++i)
    {
        for (uint64_t j = 0; j < Src.n_rows; j++)
        {
            signVectors[i][j] = 1.0;
        }
    }
}

void CentroidDecomposition::performDecomposition(std::vector<double> *centroidValues,
                                                 bool stopOnIncompleteRank /*= false*/, bool skipSSV /*= false*/)
{
    arma::mat X(Src);
    
    for (uint64_t i = 0; i < truncation; i++)
    {
        arma::vec &Z = skipSSV
                    ? signVectors[i]
                    : findSignVector(X, i);
        
        //std::cout << Z.toString() << std::endl;

        if (!decomposed && skipSSV)
        {
            directions[i] = (X.t() * Z);
        }
        
        // C_*i = X^T * Z, cached by SSV search
        arma::vec Rel_i = directions[i];
        
        // R_*i = C_*i / ||C_*i||
        double centroid = arma::norm(Rel_i);
        if (centroidValues != nullptr)
        {
            if (stopOnIncompleteRank && centroid < eps) // incomplete rank, don't even proceed with current iteration
            {
                break;
            }
            
            centroidValues->emplace_back(centroid);
        }
        
        Rel_i /= centroid;
        
        // R = Append(R, R_*i)
        Algebra::Operations::insert_vector_at_column(Rel, i, Rel_i);
        
        // L_*i = X * R
        arma::vec Load_i = X * Rel_i;
        
        // L = Append(L, L_*i)
        Algebra::Operations::insert_vector_at_column(Load, i, Load_i);
        
        // X := X - L_*i * R_*i^T
        X -= (Load_i * Rel_i.t());
    }
    
    addedRows = 0;
    decomposed = true;
}

//
// CentroidDecomposition algorithm
//

void CentroidDecomposition::increment(const arma::vec &vec)
{
    Algebra::Operations::increment_matrix(Src, vec);
    Algebra::Operations::increment_matrix(Load, arma::zeros<arma::mat>(Load.n_cols)); // doesn't matter, will be overwritten
    ++addedRows;
    
    for (uint64_t i = 0; i < Src.n_cols; ++i)
    {
        Algebra::Operations::increment_vector(signVectors[i], 1.0);
    }
}

void CentroidDecomposition::increment(const std::vector<double> &vec)
{
    increment(arma::vec(vec));
}

void CentroidDecomposition::increment_raw(uint64_t newrows)
{
    Src.resize(Src.n_rows + newrows, Src.n_cols);
    Load.resize(Load.n_rows + newrows, Load.n_cols);
    
    addedRows += newrows;
    
    for (uint64_t j = 0; j < Src.n_cols; ++j)
    {
        signVectors[j].resize(signVectors[j].n_elem + newrows);
        
        for (uint64_t i = signVectors[j].n_rows - newrows; i < signVectors[j].n_rows; ++i)
        {
            signVectors[j][i] = 1.0;
        }
    }
}

//
// Algorithm
//

arma::vec &CentroidDecomposition::findSignVector(arma::mat &mx, uint64_t k)
{
    switch (strategy)
    {
        case CDSignVectorStrategy_2::ISSVBase:
            return findIncrementalSSV(mx, k);
        
        case CDSignVectorStrategy_2::ISSVPlusBase:
            return findIncrementalSSVPlus(mx, k);
        
        case CDSignVectorStrategy_2::ISSVInit:
        case CDSignVectorStrategy_2::ISSVPlusInit:
            return findOptimizedSSV(mx, k);
        
        case CDSignVectorStrategy_2::LSVBase:
            return findLocalSignVector(mx, k, true);
        
        case CDSignVectorStrategy_2::LSVNoInit:
            return findLocalSignVector(mx, k, false);
    
        default:
            throw std::runtime_error("invalid strategy");
    }
}

arma::vec &CentroidDecomposition::findLocalSignVector(arma::mat &mx, uint64_t k, bool useInit)
{
    arma::vec &Z = signVectors[k]; // get a reference
    arma::vec &direction = directions[k];
    
    //
    // First pass - init
    //
    if (!decomposed && useInit)
    {
        for (uint64_t j = 0; j < mx.n_cols; ++j)
        {
            direction[j] = mx.at(0, j);
        }
        
        for (uint64_t i = 1; i < mx.n_rows; ++i)
        {
            double gradPlus = 0.0;
            double gradMinus = 0.0;
            
            for (uint64_t j = 0; j < mx.n_cols; ++j)
            {
                double localModPlus = direction[j] + mx.at(i, j);
                gradPlus += localModPlus * localModPlus;
                double localModMinus = direction[j] - mx.at(i, j);
                gradMinus += localModMinus * localModMinus;
            }
            
            // if keeping +1 as a sign yields a net negative to the
            Z[i] = gradPlus > gradMinus ? 1 : -1;
            
            for (uint64_t j = 0; j < mx.n_cols; ++j)
            {
                direction[j] += Z[i] * mx.at(i, j);
            }
        }
    }
    else // Alternative first pass - init to {+1}^n
    {
        direction = (mx.t() * Z);
    }
    
    //
    // 2+ pass - update to Z
    //

    // special step - check for increment, assume Z is incremented by {+1}^AR
    if (addedRows > 0)
    {
        for (uint64_t i = mx.n_rows - addedRows; i < mx.n_rows; ++i)
        {
            direction += mx.row(i).t();
        }
    }
    
    bool flipped;
    double lastNorm = // cache the current value of (||D||_2)^2 to avoid recalcs
            arma::dot(direction, direction) + eps; // eps to avoid "parity flip"
    
    do
    {
        flipped = false;
        
        for (uint64_t i = 0; i < mx.n_rows; ++i)
        {
            double signDouble = Z[i] * 2;
            double gradFlip = 0.0;
            
            for (uint64_t j = 0; j < mx.n_cols; ++j)
            {
                double localMod = direction[j] - signDouble * mx.at(i, j);
                gradFlip += localMod * localMod;
            }
            
            if (gradFlip > lastNorm) // net positive from flipping
            {
                flipped = true;
                Z[i] *= -1;
                lastNorm = gradFlip + eps;
                
                for (uint64_t j = 0; j < mx.n_cols; ++j)
                {
                    direction[j] -= signDouble * mx.at(i, j);
                }
            }
        }
    } while (flipped);
    
    return Z;
}

arma::vec &CentroidDecomposition::findOptimizedSSV(arma::mat &mx, uint64_t k)
{
    if (!decomposed)
    {
        arma::vec &Z = signVectors[k]; // get a reference
        
        std::vector<double> direction = std::vector<double>(mx.n_cols);
        
        for (uint64_t j = 0; j < mx.n_cols; ++j)
        {
            direction[j] = mx.at(0, j);
        }
        
        for (uint64_t i = 1; i < mx.n_rows; ++i)
        {
            double gradPlus = 0.0;
            double gradMinus = 0.0;
            
            for (uint64_t j = 0; j < mx.n_cols; ++j)
            {
                double localModPlus = direction[j] + mx.at(i, j);
                gradPlus += localModPlus * localModPlus;
                double localModMinus = direction[j] - mx.at(i, j);
                gradMinus += localModMinus * localModMinus;
            }
            
            double sign = gradPlus > gradMinus ? 1 : -1;
            Z[i] = sign;
            
            for (uint64_t j = 0; j < mx.n_cols; ++j)
            {
                direction[j] += sign * mx.at(i, j);
            }
        }
    }
    
    return strategy == CDSignVectorStrategy_2::ISSVInit ? findIncrementalSSV(mx, k) : findIncrementalSSVPlus(mx, k);
}

arma::vec &CentroidDecomposition::findIncrementalSSVPlus(arma::mat &mx, uint64_t k)
{
    // Scalable Sign Vector
    uint64_t pos = minusone;
    double val = 0.0;
    
    arma::vec &Z = signVectors[k]; // get a reference
    
    arma::vec S(Src.n_cols);
    arma::vec V(Src.n_rows);
    
    // pre-process rows of X
    std::vector<arma::vec> x_ = std::vector<arma::vec>();
    
    for (uint64_t i = 0; i < mx.n_rows; ++i)
    {
        x_.emplace_back(mx.row(i).t());
    }
    
    
    // ITERATION #1
    
    // S = Sum(1:n) { z_i * (X_i* ^ T) }
    
    // i = 0
    {
        for (uint64_t j = 0; j < S.n_elem; ++j)
        {
            S[j] = x_[0][j] * Z[0];
        }
    }
    
    for (uint64_t i = 1; i < mx.n_rows; ++i)
    {
        for (uint64_t j = 0; j < S.n_elem; ++j)
        {
            S[j] += x_[i][j] * Z[i];
        }
    }
    
    
    // v_i = z_i * (z_i * X_i* * S - X_i* * (X_i*)^T)
    for (uint64_t i = 0; i < mx.n_rows; ++i)
    {
        V[i] = Z[i] * (
                Z[i] * arma::dot(x_[i], S) - arma::dot(x_[i], x_[i])
        );
    }
    
    // Search next element
    
    for (uint64_t i = 0; i < mx.n_rows; ++i)
    {
        if (Z[i] * V[i] < 0)
        {
            if (fabs(V[i]) > val)
            {
                val = fabs(V[i]);
                pos = i;
            }
        }
    }
    
    // ITERATIONS 2+
    
    // main loop
    while (pos != minusone)
    {
        // Search next element to flip
        val = eps;
        pos = minusone;
        
        for (uint64_t i = 0; i < mx.n_rows; ++i)
        {
            if (Z[i] * V[i] < 0)
            {
                if (fabs(V[i]) > val)
                {
                    val = fabs(V[i]);
                    pos = i;
                    
                    // change sign
                    Z[pos] = Z[pos] * (-1.0);
                    
                    double factor = Z[pos] + Z[pos];
                    
                    // Determine V_k+1 from V_k
                    
                    for (uint64_t l = 0; l < mx.n_rows; ++l)
                    {
                        V[l] = V[l] + factor * (l == pos ? 0 : arma::dot(x_[l], x_[pos]));
                    }
                }
            }
        }
        ++ssvIterations;
    }
    
    return Z;
}

arma::vec &CentroidDecomposition::findIncrementalSSV(arma::mat &mx, uint64_t k)
{
    // Scalable Sign Vector
    uint64_t pos = minusone;
    double val = 0.0;
    
    arma::vec &Z = signVectors[k]; // get a reference
    
    arma::vec S(Src.n_cols);
    arma::vec V(Src.n_rows);
    
    // pre-process rows of X
    std::vector<arma::vec> x_ = std::vector<arma::vec>();
    
    for (uint64_t i = 0; i < mx.n_rows; ++i)
    {
        x_.emplace_back(mx.row(i).t());
    }
    
    // ITERATION #1
    
    // S = Sum(1:n) { z_i * (X_i* ^ T) }
    
    // i = 0
    {
        for (uint64_t j = 0; j < S.n_elem; ++j)
        {
            S[j] = x_[0][j] * Z[0];
        }
    }
    
    for (uint64_t i = 1; i < mx.n_rows; ++i)
    {
        for (uint64_t j = 0; j < S.n_elem; ++j)
        {
            S[j] += x_[i][j] * Z[i];
        }
    }
    
    
    
    // v_i = z_i * (z_i * X_i* * S - X_i* * (X_i*)^T)
    for (uint64_t i = 0; i < mx.n_rows; ++i)
    {
        V[i] = Z[i] * (
                Z[i] * arma::dot(x_[i], S) - arma::dot(x_[i], x_[i])
        );
    }
    
    // Search next element
    
    for (uint64_t i = 0; i < mx.n_rows; ++i)
    {
        if (Z[i] * V[i] < 0)
        {
            if (fabs(V[i]) > val)
            {
                val = fabs(V[i]);
                pos = i;
            }
        }
    }

    // ITERATIONS 2+
    
    // main loop
    while (pos != minusone)
    {
        // change sign
        Z[pos] = Z[pos] * (-1.0);
        
        // Determine V_k+1 from V_k
        
        for (uint64_t i = 0; i < mx.n_rows; ++i)
        {
            V[i] += 2 * Z[pos] * (i == pos ? 0 : arma::dot(x_[i], x_[pos]));
        }

        // Search next element to flip
        val = 0.0;
        pos = minusone;
        
        for (uint64_t i = 0; i < mx.n_rows; ++i)
        {
            if (Z[i] * V[i] < 0)
            {
                if (fabs(V[i]) > val)
                {
                    val = fabs(V[i]);
                    pos = i;
                }
            }
        }
        ++ssvIterations;
    }
    
    return Z;
}

std::pair<arma::mat, arma::mat>
CentroidDecomposition::PerformCentroidDecomposition(arma::mat &mx, uint64_t k)
{
    k = k == 0 ? mx.n_cols : k;
    CentroidDecomposition cd(mx);
    
    cd.truncation = k;
    
    cd.performDecomposition(nullptr);
    
    return std::make_pair(cd.stealLoad(), cd.stealRel());
}

// volatile function because there's an external cast from an int involved

bool isValidStrategy_2(CDSignVectorStrategy_2 _strategy)
{
    size_t code = (size_t)_strategy;
    CDSignVectorStrategy_2 strategy = (CDSignVectorStrategy_2)code;
    volatile bool valid = false;
    valid = valid;
    
    switch (strategy)
    {
        case CDSignVectorStrategy_2::ISSVBase:
        case CDSignVectorStrategy_2::ISSVPlusBase:
        case CDSignVectorStrategy_2::ISSVInit:
        case CDSignVectorStrategy_2::ISSVPlusInit:
        case CDSignVectorStrategy_2::LSVBase:
        case CDSignVectorStrategy_2::LSVNoInit:
            valid = true;
    }
    
    return valid;
}

} // namespace Algorithms
