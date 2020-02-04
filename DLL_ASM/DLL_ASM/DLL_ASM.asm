.CODE	

;------------------------------------------------------
; Projekt JA - Kalkulator uk³adów równañ (metoda LU)
; Daniel Winkler sem.5 grp.1
; Wersja: 1.4
;-------------------------------------------------------

;-------------------------------------------------------------------------------
; Procedure that calculates set of equations using LU method
; REGISTERS used:
; rcx - size of matrixes - passed by parameter
; rdx - pointer to the first element of matrix L - passed by parameter
; r8 - pointer to the first first element of matrix U - passed by parameter
; r9 - pointer to the first first element of vector Y - passed by parameter
; r10 - pointer to the first first element of vector B - taken from stack
; r11 - pointer to the first first element of vector X - taken from stack
; r12 - i counter
; r13 - k counter
; r14 - stores 0
; r15 - used to calculate matrix shifts
; xmm0 - sum
; xmm1, xmm2 - used for calculations
;-------------------------------------------------------------------------------

calculateASM PROC

;taking off additional arguments from stack
mov r10, [rsp+200+32]					   ;r10 = vecB[0]
mov r11, [rsp+200+40]					   ;r11 = vecX[0]

;saving registers on stack
push rbx
push rbp
push rdi
push rsi
push r12
push r13
push r14
push r15

mov r14, 0									;stroing 0 for future use

vmovsd xmm1, qword ptr [r10]				;xmm1 = vec_B[0]
movsd qword ptr [r9], xmm1					;vec_Y[0] = vec_B[0]

mov r12, 1									;int i = 1
for1:										;for (int i = 1; i < size; i++)
	cmp r12, rcx							;i < size
	je endfor1								;end for if i = size	

	cvtsi2sd xmm0, r14						;nulling xmm0(sum)

	mov r13, r14							;int k = 0
	for2:									;for (int k = 0; k < i; k++)
		cmp r13, r12						;k < i
		je endfor2							;end for if k = size

		mov r15, r12						;i
		imul r15, rcx						;i*size
		add r15, r13						;i*size+k
		vmovsd xmm1, qword ptr[r15*8 + rdx]	;xmm1 = mat_L[0+(i*size + k)]

		vmovsd xmm2, qword ptr[r13*8 + r9]	;xmm2 = vec_Y[0+k]

		mulsd xmm1, xmm2					;mat_L[i*size + k] * vec_Y[k]
		addsd xmm0, xmm1					;sum += mat_L[i*size + k] * vec_Y[k]

		inc r13								;k++
	jmp for2								;true -> for2
	endfor2:

	vmovsd xmm1, qword ptr [r12*8 + r10]	;xmm1 = vec_B[0+i]
	subsd xmm1, xmm0						;xmm1 = vec_B[i] - sum

	movsd qword ptr[r12*8 + r9], xmm1		;vec_Y[0+i] = vec_B[i] - sum;

	inc r12									;i++
jmp for1									;true -> for1
endfor1:

mov rsi, rcx								;size
sub rsi, 1									;size - 1
vmovsd xmm1, qword ptr [rsi*8 + r9]			;xmm1 = vec_Y[0+(size - 1)]

mov r15, rsi								;size
imul r15, rcx								;(size-1)*size
add r15, rcx								;(size-1)*size + size
sub r15, 1									;(size-1)*size + size - 1
vmovsd xmm2, qword ptr[r15*8 + r8]			;xmm2 = mat_U[0+((size - 1) * size + (size - 1))]

divsd xmm1, xmm2							;vec_Y[size - 1] / mat_U[(size - 1) * size + (size - 1)]
movsd qword ptr [rsi*8 + r11], xmm1			;vec_X[0+(size - 1)] = vec_Y[size - 1] / mat_U[(size - 1) * size + (size - 1)]

mov r12, rcx								;i = size
sub r12, 2									;i = size-2
for3:										;for (int i = size - 2; i >= 0; i--)
	cmp r12, r14							;i >= 0
	jl endfor3								;jmp if i < 0

	cvtsi2sd xmm0, r14						;double sum = 0.0

	mov r13, r12							;k = i					
	add r13, 1								;int k = i + 1
	for4:
		cmp r13, rcx						;k < size
		je endfor4							;if k = size

		mov r15, rcx						;size
		imul r15, r12						;size*i		
		add r15, r13						;(size*i) + l
		vmovsd xmm1, qword ptr [r15*8 + r8]	;xmm1 = mat_U[0+(i*size + k)]

		vmovsd xmm2, qword ptr [r13*8 + r11];xmm2 = vec_X[0+k]

		mulsd xmm1, xmm2					;mat_U[i*size + k] * vec_X[k]
		addsd xmm0, xmm1					;sum += mat_U[i*size + k] * vec_X[k]

		inc r13								;k++
	jmp for4
	endfor4:

	vmovsd xmm1, qword ptr [r12*8 + r9]		;xmm1 = vec_Y[0+i]
	subsd xmm1, xmm0						;vec_Y[i] - sum
	mov r15, r12							;i
	imul r15, rcx							;i*size
	add r15, r12							;(i*size) + i
	movsd xmm2, qword ptr [r15*8 + r8]		;xmm2 = mat_U[0+(i*size + i)]
	divsd xmm1, xmm2						;(vec_Y[i] - sum) / mat_U[i*size + i]	
	vmovsd qword ptr [r12*8 + r11], xmm1	;vec_X[0+i] = (vec_Y[i] - sum) / mat_U[i*size + i]
	dec r12									;i--
jmp for3
endfor3:
;recreating registers from stack
pop r15
pop r14
pop r13
pop r12
pop rsi
pop rdi
pop rbp
pop rbx
ret

calculateASM endp
END