all:
	g++ -O3 -D ARMA_DONT_USE_WRAPPER -o cmake-build-debug/incCD -Wall -Werror -Wextra -pedantic -Wconversion -Wsign-conversion -msse2 -msse3 -msse4 -msse4.1 -msse4.2 -fopenmp -std=gnu++14 main.cpp Testing.cpp Performance/Benchmark.cpp MathIO/MatrixReadWrite.cpp Algebra/Auxiliary.cpp Algorithms/TKCM.cpp Algorithms/SPIRIT.cpp Algorithms/GROUSE.cpp Algorithms/CDMissingValueRecovery.cpp Algebra/CentroidDecomposition.cpp Algorithms/OGDImpute.cpp Algorithms/PCA_MME.cpp Algebra/RSVD.cpp Stats/Correlation.cpp -lopenblas -larpack

mac:
	/usr/local/opt/llvm/bin/clang++ -O3 -D ARMA_DONT_USE_WRAPPER -o cmake-build-debug/incCD -Wall -Werror -Wextra -pedantic -Wconversion -Wsign-conversion -msse2 -msse3 -msse4 -msse4.1 -msse4.2 -fopenmp -std=gnu++14 main.cpp Testing.cpp Performance/Benchmark.cpp MathIO/MatrixReadWrite.cpp Algebra/Auxiliary.cpp Algorithms/TKCM.cpp Algorithms/ST_MVL.cpp Algorithms/SPIRIT.cpp Algorithms/GROUSE.cpp Algorithms/NMFMissingValueRecovery.cpp Algorithms/DynaMMo.cpp Algorithms/SVT.cpp Algorithms/ROSL.cpp Algorithms/IterativeSVD.cpp Algorithms/SoftImpute.cpp Algorithms/CDMissingValueRecovery.cpp Algebra/CentroidDecomposition.cpp Algorithms/OGDImpute.cpp Algorithms/MD_ISVDAlgorithm.cpp Algorithms/PCA_MME.cpp Algebra/RSVD.cpp Stats/Correlation.cpp -L/usr/local/opt/openblas/lib -L/usr/local/opt/llvm/lib -L/usr/local/opt/lapack/lib -lopenblas -larpack -lmlpack

clean:
	rm cmake-build-debug/incCD
