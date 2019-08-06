# InCD_benchmark => TestingFramework

For dependencies and setup look in the README file in root of repository.

# Description of testing scenarios

## Basics
- all data is z-score normalized
- normalization occurs before trimming the length for tests where N_test < N
- all tests exist in runtime and precision variants (except streaming that only exists in runtime variant)
- when percentage or division is mentioned, the result is floored down to nearest integer

### Recovery
missingpercentage/{dataset}:
- fixed data length (size=1000) and column count (size=max)
- varying missing block size [10% to 80%, step 10%] starting at col 0, line 50

length/{dataset}:
- fixed column count (size=max) and missing block length (size=100), start at col 0, line 50
- varying total length of the time series [200 to 2K, step 200]

columns/{dataset}:
- fixed data length (size=1000) and missing block (size=100), starting at col 0, line 50
- varying the amount of columns [4 to 12 | only datasets with 12 columns]

blockslide/{dataset}:
- fixed data length (size=1000), column count (size=max) and missing block size (size=200)
- varying the starting position of missing block [5% to 80%, step 5%] from the top of time series at col 0

blackout/{dataset}:
- fixed data length (size=1000) and column count (size=max)
- varying the amount of rows that are missing from all times series [10 to 100, step 10] starting at line 50

multicol-disjoint/{dataset}:
- fixed data length (size=1000) and column count (size=max)
- varying the amount of columns [1 to 4, step 1] that contain a missing block; if a column is containing a missing block it's of size = 200, starting at line=(200 + col_idx * 200)

multicol-overlap/{dataset}:
- fixed data length (size=1000) and column count (size=max)
- varying the amount of columns [1 to 4, step 1] that contain a missing block; if a column is containing a missing block it's of size = 300, starting at line=(200 + col_idx * 200)

mcar-element/{dataset}:
- fixed data length (size=1000) and column count (size=max)
- varying the percentage [10% to 80%, step 10%] of all data points in all time series that are removed; removal is uniform except it ensures at least one element in each time series is left; PRNG seed is fixed so between runs the same elements are removed

mcar-block/{dataset}:
- fixed data length (size=1000) and column count (size=max)
- varying the amount [1 to max, step 1] of columns that will contain missing blocks; missing block have a static size=10 are introduced into involved time series at random until a total of 10% of all values from them are removed; removal is uniform except it ensures at least one element in each time series is left; PRNG seed is fixed so between runs the same elements are removed

### Continuous

missingpercentage/{dataset}:
- fixed data length (size=1000) and column count (size=max)
- varying missing block size [10% to 80%, step 10%] starting at col 0, line=max-block_size

length/{dataset}:
- fixed column count (size=max) and missing block length (size=100), start at col 0, line=max-block_size
- varying total length of the time series [200 to 2K, step 200]

columns/{dataset}:
- fixed data length (size=1000) and missing block (size=100), starting at col 0, line=max-block_size
- varying the amount of columns [4 to 12 | only datasets with 12 columns]

blackout/{dataset}:
- fixed data length (size=max) and column count (size=max)
- varying the amount of rows that are missing from all times series [10 to 100, step 10] starting at line=max-block_size

### Streaming

- only a subset of algorithms is running streaming test
    - InCD (streaming variant of CDRec)
    - TKCM
    - SPIRIT
    - SAGE (streaming variant of GROUSE)
    - OGDImpute
    - MD_ISVD
- others don't have explicit streaming/update option that can improve performance compared to normal runtime test

missingpercentage/{dataset}:
- fixed data length (size=1000) and column count (size=max)
- varying missing block size [10% to 80%, step 10%] starting at col 0, line=max-block_size
- algorithms pre-process N - block_size rows first, then new data is given and its processing is measured

length/{dataset}:
- fixed column count (size=max) and missing block length (size=100), start at col 0, line=max-block_size
- varying total length of the time series [200 to 2K, step 200]
- algorithms pre-process N - block_size rows first, then new data is given and its processing is measured

columns/{dataset}:
- fixed data length (size=1000) and missing block (size=100), starting at col 0, line=max-block_size
- varying the amount of columns [4 to 12 | only datasets with 12 columns]
- algorithms pre-process N - block_size rows first, then new data is given and its processing is measured
