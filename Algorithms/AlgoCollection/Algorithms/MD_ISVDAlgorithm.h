//
// Created by zakhar on 24/07/19.
//

#pragma once

#include <armadillo>

namespace Algorithms
{

class MD_SVD
{
  private:
    arma::mat &input;
    std::vector<arma::uvec> indices;
    arma::mat U;
    arma::vec S;
    arma::mat R;
    
    uint64_t maxrank;
    uint64_t lastIndex;
  
  public:
    explicit MD_SVD(arma::mat &_input, uint64_t _maxrank);
  
  public:
    void doMDISVD();
};

} // namespace Algorithms


