# InCD_benchmark => Algorithms

For dependencies and setup look in the README file in root of repository.

# Description of parameters

## ST-MVL

Parameters:
- windowSize = 3
- alpha = 4 (default)
- gamma = 0.85 (default)

## SPIRIT

Parameters (suggested values):
- lambda = 1.0
- k0 = 3 (hidden variables)
- w = 6 (AR coefficients)

## TKCM

Parameters:
- recovery
	- k = 3
	- l = 20 (higher values cause SegFaults due to low amount of historical data)
	- d = 3
- prediction
	- k = 3
	- l = 30 (same as above)
	- d = 3

## TRMF

Parameters:
- lag_idx = array(1...10)
- k = M * 2 (changes with varying M for column-test)
- learning_factors = [0.75, 0.75, 0.75]
- max_iterations = 100

## NNMF

Parameters:
- n_components = 3
- eps = 1E-6
- max_iterations = 100

## GROUSE

Parameters:
- truerank = 3;
- maxrank = 3;
- maxit = 100;
- maxCycles = 5;
- step_size = 0.1;
