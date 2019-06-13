//
// Created by zakhar on 04/01/19.
//

#pragma once

#include <armadillo>

namespace Algorithms
{

class GROUSE
{
  private:
    arma::mat &input;
    arma::mat U;
    arma::mat R;
    
    uint64_t maxrank;
  
  public:
    explicit GROUSE(arma::mat &_input, uint64_t _maxrank);
    
  public:
    void doGROUSE();
    void singleRowIncrementSAGE();
  
  private:
    static constexpr uint64_t maxCycles = 5;
    static constexpr double step_size = 0.1;
};

} // namespace Algorithms

