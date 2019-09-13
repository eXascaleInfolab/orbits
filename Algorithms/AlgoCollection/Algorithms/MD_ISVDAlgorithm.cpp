//
// Created by zakhar on 24/07/19.
//

#include <cmath>
#include <iostream>

#include "MD_ISVDAlgorithm.h"
#include "../Algebra/Auxiliary.h"

namespace Algorithms
{

MD_SVD::MD_SVD(arma::mat &_input, uint64_t _maxrank)
        : input(_input), maxrank(_maxrank), lastIndex(0)
{ }

void Algorithms::MD_SVD::doMDISVD()
{
    arma::arma_rng::set_seed(1921);
    
    for (uint64_t i = lastIndex; i < input.n_cols; ++i)
    {
        indices.emplace_back(arma::find_finite(input.col(i)));
    }
    
    if (lastIndex == 0)
    {
        U = arma::orth(arma::randn<arma::mat>(input.n_rows, maxrank));
        S = arma::zeros(maxrank);
    }
    
    for (uint64_t k = lastIndex; k < input.n_cols; ++k)
    {
        // Pull out the relevant indices and revealed entries for this column
        arma::uvec &idx = indices[k];//find(Indicator(:,col_order(k)));
        arma::vec currentCol = input.col(k);
        arma::vec v_Omega = currentCol.elem(idx);
        arma::mat U_Omega = U.rows(idx);
        
        // Predict the best approximation of v_Omega by u_Omega.
        // That is, find weights to minimize ||U_Omega*weights-v_Omega||^2
        
        arma::vec weights;
        bool success = arma::solve(weights, U_Omega, v_Omega);
        
        if (!success)
        {
            std::cout << "arma::solve has failed, aborting remaining recovery" << std::endl;
            return;
        }
        // Compute the residual not predicted by the current estmate of U.
        
        arma::vec p = U_Omega * weights;
        arma::vec residual = arma::zeros(input.n_rows);
        residual(idx) = v_Omega - p;
        double norm_residual = arma::norm(residual);
    
        arma::mat center_matrix = arma::diagmat(S);
    
        Algebra::Operations::increment_matrix(center_matrix, arma::zeros<arma::vec>(weights.n_elem));
        Algebra::Operations::increment_vector(weights, norm_residual);
        Algebra::Operations::add_matrix_col(center_matrix, weights); // weights are mutated to match
    
        arma::mat U_svd, V_svd;
        arma::vec Sigma_svd;
    
        success = arma::svd_econ(U_svd, Sigma_svd, V_svd, center_matrix);
        if (!success)
        {
            std::cout << "arma::svd has failed, aborting remaining recovery" << std::endl;
            return;
        }
        
        double r_div = std::max(norm_residual, 1E-12);
        
        residual /= r_div;
    
        Algebra::Operations::add_matrix_col(U, residual);
        
        U = U * U_svd;
        
        U = U.submat(arma::span::all, arma::span(0, maxrank - 1));
        S = Sigma_svd.subvec(arma::span(0, maxrank - 1));
    }
    
    // generate R
    
    R = arma::mat(input.n_cols, maxrank);
    
    for (uint64_t k = 0; k < input.n_cols; ++k)
    {
        // Pull out the relevant indices and revealed entries for this column
        arma::uvec &idx = indices[k]; //idx = find(Indicator(:,k));
        arma::vec currentCol = input.col(k);
        arma::vec v_Omega = currentCol.elem(idx);
        arma::mat U_Omega = U.rows(idx);
        // solve a simple least squares problem to populate R
        arma::vec sol = arma::solve(U_Omega, v_Omega);
        
        for (uint64_t i = 0; i < sol.n_rows; ++i)
        {
            R(k, i) = sol[i];
        }
    }
    
    arma::mat recon = U * R.t();
    
    for (uint64_t j = lastIndex; j < input.n_cols; ++j)
    {
        for (uint64_t i = 0; i < input.n_rows; ++i)
        {
            if (std::isnan(input.at(i, j)))
            {
                input.at(i, j) = recon.at(i, j);
            }
        }
    }
    
    lastIndex = input.n_cols; // [!] NEXT ONE
}

} // namespace Algorithms
