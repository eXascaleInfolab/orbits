//
// Created by zakhar on 04/01/19.
//

#include <cmath>
#include <iostream>

#include "GROUSE.h"
#include "../Algebra/Auxiliary.h"

namespace Algorithms
{

GROUSE::GROUSE(arma::mat &_input, uint64_t _maxrank)
    : input(_input), maxrank(_maxrank)
{ }

void Algorithms::GROUSE::doGROUSE()
{
    arma::arma_rng::set_seed(1921);
    
    std::vector<arma::uvec> indices;
    
    for (uint64_t i = 0; i < input.n_cols; ++i)
    {
        indices.emplace_back(arma::find_finite(input.col(i)));
    }
    
    U = arma::orth(arma::randn<arma::mat>(input.n_rows, maxrank));
    
    for (uint64_t outiter = 0; outiter < maxCycles; ++outiter)
    {
        for (uint64_t k = 0; k < input.n_cols; ++k)
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
            
            //arma::vec weights = arma::pinv(U_Omega) * v_Omega;
            double norm_weights = arma::norm(weights); (void) norm_weights;
            
            // Compute the residual not predicted by the current estmate of U.
            
            arma::vec p = U_Omega * weights;
            arma::vec residual = v_Omega - p;
            double norm_residual = arma::norm(residual);
            
            //if (norm_residual < 0.000000001)
            //{
            //    norm_residual = 0.000000001;
            //}
            
            // This step-size rule is given by combining Edelman's geodesic
            // projection algorithm with a diminishing step-size rule from SGD.  A
            // different step size rule could suffice here...
            
            // this is matlab version
            #if true
            {
                double sG = norm_residual*norm_weights;
                if (norm_residual < 0.000000001)
                {
                    sG = 0.000000001 * norm_weights;
                }
                //err_reg((outiter-1)*numc + k) = norm_residual/norm(v_Omega);
                double t = step_size*sG/(double)( (outiter)*input.n_cols + k + 1 );
                
                // Take the gradient step.
                if (t < (arma::datum::pi / 2.0)) // drop big steps
                {
                    double alpha = (cos(t) - 1.0) / std::pow(norm_weights, 2);
                    double beta = sin(t) / sG;
                    
                    arma::vec step = U * (alpha * weights);
                    
                    step.elem(idx) += (beta * residual);
                    
                    U = U + step * weights.t();
                }
            }
            #endif
            
            // this is python version
            #if false
            {
                double norm_p = arma::norm(p);
                double theta = std::atan(norm_residual/norm_p); //r_norm/p_norm
    
                arma::vec compA = (std::cos(theta) - 1.0) * p / norm_p;
                arma::vec compB = std::sin(theta) * residual / norm_residual;
                arma::vec compC = weights / norm_p; // \|w\|_2 = \|p\|_2
                
                arma::rowvec compC_T = compC.t();
                
                arma::mat out_AC = compA * compC_T;
    
                auto i = idx.begin();
                uint64_t iouter = 0;
                for ( ; i < idx.end(); ++i, ++iouter)
                {
                    for (uint64_t j = 0; j < U.n_cols; ++j)
                    {
                        U.at(*i, j) += out_AC.at(iouter, j);
                    }
                };
                //U = U + out_AC;
                
                arma::mat out_BC = compB * compC_T;
    
                //U[Omega, :] += np.outer(compB, compC)
                i = idx.begin();
                iouter = 0;
                for ( ; i < idx.end(); ++i, ++iouter)
                {
                    for (uint64_t j = 0; j < U.n_cols; ++j)
                    {
                        U.at(*i, j) += out_BC.at(iouter, j);
                    }
                };
            }
            #endif
        }
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

void GROUSE::singleRowIncrementSAGE()
{
    // basic input: U, R^T, s.t. X = U*R^T, vector with new values
    
    const uint64_t i = input.n_cols - 1; // last idx, single inc only, so no modifications within one call
    arma::uvec Omega_t = arma::find_finite(input.col(i)); // not int the list like it was in grouse, calculate now
    arma::vec v_t = input.col(i); // new vector
    
    // --- preprocessing ---
    // compute remaining input for SAGE with typical grouse step, for docs see above
    
    arma::vec v_Omega = v_t.elem(Omega_t);
    arma::mat U_Omega = U.rows(Omega_t);
    
    arma::vec weights;
    bool success = arma::solve(weights, U_Omega, v_Omega);
    
    if (!success)
    {
        std::cout << "arma::solve has failed, aborting remaining recovery" << std::endl;
        return;
    }
    
    arma::vec p = U_Omega * weights;
    arma::vec residual = v_Omega - p;
    double norm_residual = arma::norm(residual);
    if (norm_residual < 0.000000001)
    {
        norm_residual = 0.000000001;
    }
    // END typical grouse step
    
    // now we have all input: incoming vector which is now Omega'd, w_t, r_t, ||r_t||
    
    if (Omega_t.n_elem != v_t.n_elem)
    {
        arma::vec impute = U * weights;
    
        for (uint64_t j = 0; j < v_t.n_elem; ++j)
        {
            if (std::isnan(input.at(j, i)))
            {
                input.at(j, i) = impute[j];
            }
        }
    }
    
    // --- SAGE ---
    
    // step 1 : construct center_matrix (aka "extended Sigma") and compute its SVD
    arma::mat center_matrix = arma::eye<arma::mat>(weights.n_elem, weights.n_elem);
    
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
    
    U_svd = U_svd.cols(arma::span(0, maxrank - 1));
    V_svd = V_svd.cols(arma::span(0, maxrank - 1));
    arma::mat Sigma_mat = arma::diagmat(Sigma_svd.subvec(arma::span(0, maxrank - 1)));
    
    // step 2 : perform update on U
    residual /= norm_residual;
    
    Algebra::Operations::add_matrix_col(U_Omega, residual);
    U_Omega = U_Omega * U_svd;
    U.rows(Omega_t) = U_Omega;
    
    // step 3 : perform update on R
    
    Algebra::Operations::add_matrix_col(R, arma::zeros<arma::mat>(R.n_rows)); // add a column of 0
    Algebra::Operations::increment_matrix(R, Algebra::Predefined::canonical_vector(R.n_cols, R.n_cols - 1)); // add a row of 0 ... 0 1
    
    R = R * V_svd * Sigma_mat;
    
    // step 4 : recover missing values in new data
    
    arma::vec newdata = U * R.row(i).t();
}

} // namespace Algorithms
