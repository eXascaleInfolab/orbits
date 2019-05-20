//
// Created by zakhar on 17/05/19.
//

#pragma once

#include <armadillo>

namespace Algorithms
{

class OGDImpute
{
    //
    // Data
    //
  private:
    arma::mat &matrix;
    uint64_t p;
    
    std::vector<double> ballK;
    uint64_t lastIdx;
    
  public:
    explicit OGDImpute(arma::mat &_X, uint64_t _p);
    
    void ARPredict();
  
  private:
    arma::vec coeff_lmse_retrain(uint64_t j, uint64_t i);
    
    void OGDImpute_call(arma::mat &X, uint64_t j);
    
};

} // namespace Algorithms
