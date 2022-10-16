run : test
	./test
test: test.o
	gcc -o test test.o -no-pie
test.o: test.asm
	nasm -f elf64 -g -F dwarf test.asm -l test.lst