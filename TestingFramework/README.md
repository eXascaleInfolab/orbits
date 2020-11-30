# Description of recovery scenarios

## Setup
- all data is z-score normalized
- normalization occurs before trimming the length for tests where N_test < N
- when percentage or division is mentioned, the result is floored down to nearest integer

## Scenarios
N = lentgh of time series 
M = number of time series

W = 10% * N

### I. Batch End

missingpercentage:
- N = max; M = max;
- size of the missing block varies between 10% and 80% of the series; position: end of 1st series.

length:
- M = max; N varies between 20% and 100% of max(N);
- size of the missing block = W; position: end of 1st series.

columns:
- N = max; M = varies between min(10% of columns, 4) and 100% the number of columns;
- size of the missing block = W;  position: end of 1st series.

blackout:
- N = max; M = max;
- Missing data varies between 10 and 100 values of each series; position: end of series.

multicol-increasing:
- N = max; M = max;
- number of incomplete series varies between 10% and 100% of max(M); size of missing blocks= W; position: at the end of an incomplete time series.

mcar-ts-block\*:
- N = max; M = max;
- number of incomplete series varies between 10% and 100% of max(M); Missing block: 1 per incomplete series, size missing block = 10% * W, position: random within W from the end.

mcar-ts-block2\*:
- same as ts-block, but only every second incomplete time series.

mcar-ts-multiblock\*:
- N = max; M = max;
- number of incomplete series varies between 10% and 100% of Max(M); sub-blocks of 2% * W are removed at random from each incomplete series until a total of 10% * W of all points in the series are missing. position: random within W from the end.

mcar-ts-multiblock2\*:
- same as ts-multiblock, but only every second incomplete time series.

mcar-columns\*:
- N = max; M = varies between max(10% of columns, 4) and  100% of columns;
- Missing blocks: 10% * W of every second series total in sub-blocks of 1% * W, start = random within W from the end.

mcar-length\*:
- M = max; N varies between 20% and 100% of max(N);
- Missing blocks:  10% * W of every second series total in sub-blocks of 1% * W, start = random within W from the end.

### II. Streaming End

missingpercentage:
- N = max; M = max;
- size of the missing block varies between 10% and 80% of the series; position: end of 1st series.

length:
- M = max; N varies between 20% and 100% of max(N);
- size of the missing block = W; position: end of 1st series.

columns:
- N = max; M = varies between min(10% of columns, 4) and 100% the number of columns;
- size of the missing block = W;  position: end of 1st series.

blackout:
- N = max; M = max;
- Missing data varies between 10 and 100 values of each series; position: end of series.

multicol-increasing:
- N = max; M = max;
- number of incomplete series varies between 10% and 100% of max(M); size of missing blocks= W; position: at the end of an incomplete time series.

mcar-ts-block\*:
- N = max; M = max;
- number of incomplete series varies between 10% and 100% of max(M); Missing block: 1 per incomplete series, size missing block = 10% * W, position: random within W from the end.

mcar-ts-block2\*:
- same as ts-block, but only every second incomplete time series.

mcar-ts-multiblock\*:
- N = max; M = max;
- number of incomplete series varies between 10% and 100% of Max(M); sub-blocks of 2% * W are removed at random from each incomplete series until a total of 10% * W of all points in the series are missing. position: random within W from the end.

mcar-ts-multiblock2\*:
- same as ts-multiblock, but only every second incomplete time series.

mcar-columns\*:
- N = max; M = varies between max(10% of columns, 4) and  100% of columns;
- Missing blocks: 10% * W of every second series total in sub-blocks of 1% * W, start = random within W from the end.

mcar-length\*:
- M = max; N varies between 20% and 100% of max(N);
- Missing blocks:  10% * W of every second series total in sub-blocks of 1% * W, start = random within W from the end.

<!--

### III. Batch Mid

missingpercentage:
- N = max; M = max;
- size of a single single block varies between 10% and 80% of the series, position: at 5% of 1st series from the top.

length:
- M = max; N varies between 20% and 100% of the series;
- size of a single missing block is 10% * N, position: at 5% of 1st series from the top.

columns:
- N = max; M = varies from min(10% of columns, 4) to 100% of columns;
- size of a single missing block is W, position: at 5% of 1st series from the top.

blackout:
- N = max; M = max;
- Missing data - 10 to 100 rows in each time series, position: at 5% of all series from the top.

multicol-increasing:
- N = max; M = max;
- Missing data - 10 to 100% time series are incomplete; Missing blocks are of size W, position: at 5% of all series from the top.

multicol-disjoint:
- N = max; M = max;
- Missing data - size = N/M, position: in each time series = column_index * size

multicol-overlap:
- N = max; M = max;
- Missing data - size = 2 * N/M for all columns except last; last column = N/M; position: in each time series = column_index * (size/2)

mcar-matrix-block\*:
- N = fixed; M = fixed; Missing data - randomly generated 10% to 80% of all values in all time series.

mcar-ts-block\*:
- N = max; M = max;
- Missing data - 10 to 100% time series are incomplete; Missing block - 1 per incomplete time series, size = 10% * W, position: random except top 20% of the series.

mcar-ts-block2\*:
- same as ts-block, but only every second incomplete time series.

mcar-ts-multiblock\*:
- N = max; M = max;
- Missing data - 10 to 100% time series are incomplete; sub-blocks of 2% * W are removed at random from each incomplete TS until a total of 10% * W of all points in this TS are missing. position: random except top 20% of the series.

mcar-ts-multiblock2\*:
- same as ts-multiblock, but only every second incomplete time series.

mcar-columns\*:
- N = max; M = varies from max(10% of columns, 4) to 100% of columns, step 10%;
- Missing blocks - 10% * W of every second time series total in sub-blocks of 1% * W, start = random except top 20% of the series.

mcar-length\*:
- M = max; N varies from 20% of the size to 100% of the size by steps of 10%;
- Missing blocks - 10% * W of every second time series total in sub-blocks of 1% * W, start = random except top 20% of the series.

-->

### Remarks

\* mcar scenario uses random number generator with fixed seed and will produce the same blocks every run
