section .data
section .bss
section .text
	 global main
main:
push 20
push 4
pop rbx
pop rax
xor rdx, rdx
idiv rbx
push rax
mov rax, 60
pop rdi
syscall
