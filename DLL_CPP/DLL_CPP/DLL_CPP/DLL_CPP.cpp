/* 
Projekt JA - Kalkulator uk³adów równañ (metoda LU)
Daniel Winkler sem.5 gr.1
Wersja: 1.2
*/

#include "stdafx.h"

extern "C"
{
	/*
	Function that calculates set of equations using LU method
	
	Parameters: 
	size - size of matrixes
	matL - pointer to first element of matrix L
	matU - pointer to first element of matrix U
	vecY - pointer to first element of vector Y
	vecB - pointer to first element of vector B
	vecX - pointer to first element of vector X
	
	Function Returns: 
	Function operates on original data, so it doesn't return any parameters. 
	It changes vector X, which containts the result of set of equations.
	*/
	__declspec(dllexport) void calculateCPP(int size, double* matL, double* matU, double* vecY, double* vecB, double* vecX)
	{
		vecY[0] = vecB[0];								   // vecY[1] = vecB[1]
																								
		for (int i = 1; i < size; i++)													
		{																				
			double sum = 0.0;
			for (int k = 0; k < i; k++)
				sum += matL[i*size + k] * vecY[k];	       // sum = E(matL[i,k]*vecY[k]); E = sigma  
			vecY[i] = vecB[i] - sum;                       // vecY[i] = vecB[i] - sum;	i = 2,1,3,...,n   
		}

		vecX[size - 1] = vecY[size - 1] / matU[(size - 1) * size + (size - 1)];		//vecX[n] = (vecY[n]/matU[n][n]); n = size
																					
		for (int i = size - 2; i >= 0; i--)											
		{																			
			double sum = 0.0;
			for (int k = i + 1; k < size; k++)
				sum += matU[i*size + k] * vecX[k];               // sum = E(matU[i][k]*vecX[k]); E = sigma  
			vecX[i] = (vecY[i] - sum) / matU[i*size + i];		 //vecX[i] = (vecY[i] - sum)/(matU[i][i])	i=n-1,n-2,...,1
		}
	}
}


