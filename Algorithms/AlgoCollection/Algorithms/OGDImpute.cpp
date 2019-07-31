//
// Created by zakhar on 17/05/19.
//

#include "OGDImpute.h"

namespace Algorithms
{

OGDImpute::OGDImpute(arma::mat &_X, uint64_t _p)
        : matrix(_X),
          p(_p),
          ballK(std::vector<double>(p)),
          lastIdx(0)
{
    const double inverseroot2 = 1 / sqrt(2.0);
    
    std::cout << "K = { ";
    for (uint64_t i = 0; i < p; ++i)
    {
        ballK[i] = std::pow(inverseroot2, i + 1);
        std::cout << ballK[i] << " ";
    }
    std::cout << "}" << std::endl;
}

void OGDImpute::ARPredict()
{
    for (uint64_t j = 0; j < matrix.n_cols; ++j)
    {
        OGDImpute_call(matrix, j);
    }
}

arma::vec OGDImpute::coeff_lmse_retrain(uint64_t j, uint64_t i)
{
    arma::vec past_vec = ((arma::vec)matrix.col(j)).subvec(arma::span(i - p, i - 1));
    arma::mat autocorr = past_vec * past_vec.t();
    
    arma::vec rxx = past_vec * matrix.at(i, j);
    
    arma::vec coeff = arma::pinv(autocorr) * rxx;
    
    return coeff;
}

//#define _OGD_IMPUTE_VERBOSE

void OGDImpute::OGDImpute_call(arma::mat &X, uint64_t j)
{
    arma::vec coeff = arma::zeros<arma::vec>(p);
    
    for (uint64_t i = 0; i < p; ++i) // todo: fix, if any ts[idx(ts) < p] are missing, they are set to 0
    {
        if (std::isnan(X.at(i, j)))
        {
            X.at(i, j) = 0.0;
        }
    }
    
    double noisestd = 0.0; // aka regret in this context
    uint64_t nonmissing_cnt = 0;
    
    for (uint64_t i = std::max(p, lastIdx); i < X.n_rows; ++i)
    {
        if (std::isnan(X.at(i, j)))
        {
            double predict_i = 0.0;
            for (uint64_t k = 0; k < p; ++k)
            {
                predict_i += X.at(i - k - 1, j) * coeff[k];
            }
            #ifdef _OGD_IMPUTE_VERBOSE
            std::cout << "predicted[missing]=" << predict_i << std::endl;
            #endif
            X.at(i, j) = predict_i;
        }
        else
        {
            double learning_rate = 1 / sqrt((double)i);
            
            double predict_i = 0.0;
            for (uint64_t k = 0; k < p; ++k)
            {
                predict_i += X.at(i - k - 1, j) * coeff[k];
            }
            double diff = (X.at(i, j) - predict_i); //grad(l_t(Xt,matrix^ar_t(a^t)))
            noisestd += diff * diff;
            nonmissing_cnt++;
            
            #ifdef _OGD_IMPUTE_VERBOSE
            std::cout << "step " << i << std::endl;
            std::cout << "[pre]  predicted=" << predict_i << "; real=" << X.at(i, j) << "; diff=" << diff << std::endl;
            #endif
    
            arma::vec new_coeff = coeff_lmse_retrain(j, i);
            
            // update alpha
            for (uint64_t k = 0; k < p; ++k)
            {
                double gradient = new_coeff[k] - coeff[k];
                
                if (!arma::is_finite(gradient))
                {
                    continue;
                }
                
                coeff[k] += learning_rate * gradient;
                
                if (coeff[k] >= ballK[k])
                {
                    coeff[k] = ballK[k];
                }
                else if (coeff[k] <= -ballK[k])
                {
                    coeff[k] = -ballK[k];
                }
            }
            
            #ifdef _OGD_IMPUTE_VERBOSE
            predict_i = 0.0;
            for (uint64_t k = 0; k < p; ++k)
            {
                predict_i += X.at(i - k - 1, j) * coeff[k];
            }
            diff = (X.at(i, j) - predict_i);
            std::cout << "new coeff_estim-d"; new_coeff.t().print();
            std::cout << "new coeff_learned"; coeff.t().print();
            std::cout << "[post] predicted=" << predict_i << "; real=" << X.at(i, j) << "; diff=" << diff << std::endl << std::endl;
            //usleep(1000 * 1000);
            #endif
        }
    }
    
    noisestd /= (double)(nonmissing_cnt - 1);
    
    #ifdef _OGD_IMPUTE_VERBOSE
    std::cout << "noise_std = " << sqrt(noisestd) << std::endl;
    #endif
    
    for (uint64_t i = 0; i < X.n_rows; ++i)
    {
        //matrix.at(i, j) -= 1.5;
    }
}
} // namespace Algorithms
