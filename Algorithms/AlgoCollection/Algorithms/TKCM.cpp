//
// Created by zakhar on 03/01/19.
//

//
// Code taken directly from: https://www.ifi.uzh.ch/en/dbtg/Staff/wellenzohn/dasa.html and adapted for C++
//

#include <cstdio>
#include <cstdlib>
#include <cmath>
#include <cstdint>
#include <iostream>
#include <limits>
#include <chrono>

#include "TKCM.h"

namespace Algorithms
{

#define POS(row, col) (((nr_patterns+1)*(row)) + (col))

TKCM::TKCM(arma::mat &mx)
        : matrix(mx)
{ }

void TKCM::actionTkcm(const arma::mat &ref_ts, arma::vec &ts, uint64_t &offset, const uint64_t &L)
{
    uint64_t nr_patterns = L - 2 * l + 1;
    arma::vec M((k + 1) * (nr_patterns + 1));
    M.zeros();
    arma::vec D(nr_patterns + 1);
    D.zeros();
    std::vector<uint64_t> A(k);
    
    // step 1: compute pattern dissimilarities
    for (uint64_t j = 1; j <= L - 2 * l + 1; ++j)
    {
        D[j] = 0;
        for (uint64_t i = 0; i < d; ++i)
        {
            for (uint64_t x = 0; x <= l - 1; x++)
            {
                uint64_t pos = offset + l + j - 1 - x;
                double x1 = ref_ts.at(i, mod(pos, L));
                double x2 = ref_ts.at(i, mod(offset - x, L));
                D[j] += pow(x1 - x2, 2);
            }
        }
        D[j] = sqrt(D[j]);
    }
    
    // step 2.1: dynamic programming
    for (uint64_t j = 0; j <= L - 2 * l + 1; ++j)
    {
        M[POS(0, j)] = 0;
        for (uint64_t i = 1; i <= k; ++i)
        {
            if (i > j)
            {
                M[POS(i, j)] = std::numeric_limits<double>::infinity();
            }
            else
            {
                uint64_t pred = j >= l ? j - l : 0;
                M[POS(i, j)] = fmin(M[POS(i, j - 1)], D[j] + M[POS(i - 1, pred)]);
            }
        }
    }
    
    // step 2.2: backtracking
    uint64_t i = k;
    uint64_t j = L - 2 * l + 1;
    while (i > 0)
    {
        if (M[POS(i, j)] == M[POS(i, j - 1)])
        {
            --j;
        }
        else
        {
            A[i - 1] = j;
            --i;
            j = j >= l ? j - l : 0;
        }
    }
    
    // step 3: impute missing value
    double sum = 0;
    for (i = 0; i < k; ++i)
    {
        uint64_t pos = offset + l + A[i] - 1;
        sum += ts[mod(pos, L)];
    }
    ts[offset] = sum / (double)k;
}

int64_t TKCM::performRecovery(bool stream)
{
    std::chrono::steady_clock::time_point begin = std::chrono::steady_clock::now();
    
    uint64_t L = matrix.n_rows - 1;
    
    for (uint64_t i = 0; i < matrix.n_rows; ++i)
    {
        if (std::isnan(matrix.at(i, 0)))
        {
            L = i;
            break;
        }
    }
    
    uint64_t cutoff10 = matrix.n_rows - (matrix.n_rows / 10);
    L = std::min(L, cutoff10);
    
    if (L < 51)
    {
        l = 20;
    }
    
    uint64_t offset = 0;
    arma::vec ts(L);
    ts.zeros();
    arma::mat ref_ts(d, L);
    ref_ts.zeros();
    
    for (uint64_t i = 0; i < matrix.n_rows; ++i, ++offset)
    {
        offset = mod(offset, L);
        ts[offset] = matrix.at(i, 0);
        ref_ts.at(0, offset) = matrix.at(i, 1);
        ref_ts.at(1, offset) = matrix.at(i, 2);
        ref_ts.at(2, offset) = matrix.at(i, 3);
        
        if (i >= L)
        {
            if (stream && i == L)
            {
                begin = std::chrono::steady_clock::now();
            }
            actionTkcm(ref_ts, ts, offset, L);
        }
    }
    
    offset = 0;
    for (uint64_t i = L; i < matrix.n_rows; ++i, ++offset)
    {
        offset = mod(offset + 1, L);
        matrix.at(i, 0) = ts[offset];
    }
    return std::chrono::duration_cast<std::chrono::microseconds>(std::chrono::steady_clock::now() - begin).count();
}

} // namespace Algorithms
