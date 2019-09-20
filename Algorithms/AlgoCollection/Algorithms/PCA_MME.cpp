//
// Created by zakhar on 05.09.19.
//

#include <cmath>
#include <iostream>

#include "PCA_MME.h"

namespace Algorithms
{

PCA_MME::PCA_MME(arma::mat &_input, uint64_t _k, bool singleBlock)
        : input(_input), m(_input.n_rows), n(_input.n_cols), t(0), C(0.25),
          delta(1.0), k(_k), order(2), id(1), done(false)
{
    if (singleBlock)
    {
        T = 1;
    }
    else
    {
        double res = (double)arma::sum(arma::sum(arma::find_finite(input)));
        delta = res / (double)input.n_elem;
        
        std::cout << "pre-ceil: " << C * std::log((double)(m * n) * delta / (double)k) << std::endl;
        
        T = (uint64_t) std::ceil(C * std::log((double)(m * n) * delta / (double)k));
    }
    
    B = n / T;
    nextBlock = 1;
    
    for (uint64_t i = B; i < B * (uint64_t)T + 1; i += B)
    {
        blocks.emplace_back(i);
    }
    blocks[blocks.size() - 1] = n;
    
    for (auto &i : blocks)
    {
        std::cout << i << ", ";
    }
    std::cout << std::endl;
    
    Q = arma::ones<arma::mat>(m, k);
    Qnew = arma::zeros<arma::mat>(m, k);
}

void Algorithms::PCA_MME::doPCA_MME()
{
    while (!done)
    {
        arma::vec sample = _getNextSample();
        if (sample.n_elem == 0)
        {
            done = true;
        }
        else
        {
            _update(sample);
        }
        
        if (done || t >= blocks[nextBlock - 1])
        {
            _orthonormalize();
            nextBlock++;
            if (nextBlock > T)
            {
                done = true;
            }
        }
    }
    
    //step2: recovery
    
    arma::mat R = arma::mat(input.n_cols, k);
    
    for (uint64_t l = 0; l < input.n_cols; ++l)
    {
        arma::vec sample = input.col(l);
    
        for (uint64_t i = 0; i < sample.n_elem; ++i)
        {
            if (std::isnan(sample[i]))
            {
                sample[i] = 0.0;
            }
        }
        // Pull out the relevant indices and revealed entries for this column
        // solve a simple least squares problem to populate R
        arma::vec sol = arma::solve(Q, sample);
        
        for (uint64_t i = 0; i < sol.n_rows; ++i)
        {
            R(l, i) = sol[i];
        }
    }
    
    arma::mat recon = Q * R.t();
    
    for (uint64_t j = 0; j < input.n_cols; ++j)
    {
        for (uint64_t i = 0; i < input.n_rows; ++i)
        {
            if (std::isnan(input.at(i, j)))
            {
                input.at(i, j) = recon.at(i, j);
            }
        }
    }
}

void Algorithms::PCA_MME::streamPCA_MME()
{
    done = false;
    T++; // we will have one more block
    blocks.emplace_back(n); // new block ends at the new end of matrix
    // nextBlock value doesn't change since it was already set 1 index past the previous end
    n = input.n_cols; // refresh value
    
    doPCA_MME();
}

void PCA_MME::_update(const arma::vec &sample)
{
    arma::mat sampleTQ = sample.t() * Q;
    arma::mat res = arma::pow(sampleTQ, (double)order - 1.);
    Qnew += sample * res;
}

void PCA_MME::_orthonormalize()
{
    Qnew /= (double)B;
    
    arma::mat R;
    (void) arma::qr_econ(Q, R, Qnew);
    
    arma::mat P = arma::diagmat(arma::sign(R.diag()));
    
    Q = Q * P;
    R = P * R;
    
    Qnew = arma::zeros<arma::mat>(m, k);
}

arma::vec PCA_MME::_getNextSample()
{
    if (t >= input.n_cols)
        return arma::vec();
    
    arma::vec sample = input.col(t);
    
    for (uint64_t i = 0; i < sample.n_elem; ++i)
    {
        if (std::isnan(sample[i]))
        {
            sample[i] = 0.0;
        }
    }
    
    t++;
    
    return sample;
}

} // namespace Algorithms
