//
// Created by zakhar on 09/01/19.
//

#pragma once

#include <tuple>
#include <armadillo>

namespace Algorithms
{

class SPIRIT
{
  public:
    static int64_t doSpirit(arma::mat &A, uint64_t k0, uint64_t w, double lambda, bool stream = false);
  
  private:
    static void grams(arma::mat &A);
    
    static void updateW(arma::vec &old_x, arma::vec &old_w, double &d, double lambda);
    
    constexpr static double sqrtEps = 1.4901e-08; //taken from octave console
};

} // namespace Algorithms



