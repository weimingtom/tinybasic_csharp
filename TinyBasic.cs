/*
 * 由SharpDevelop创建。
 * 用户： a
 * 日期: 2022/12/18
 * 时间: 10:30
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Diagnostics;


//search FIXME:
//search if (false) {
//search if (true) {
//search if (IS_DEBUG) {
namespace tinybasic
{
	/// <summary>
	/// Description of TinyBasic.
	/// </summary>
	public partial class TinyBasic
	{
		/* A tiny BASIC interpreter */
		
		// heavily modified from:
		// https://gist.github.com/pmachapman/661f0fff9814231fde48
		//
		// Now it works with doubles instead of ints. Also fixed some bugs.
		
//		#include <string.h>
//		#include <stdio.h>
//		#include <setjmp.h>
//		#include <math.h>
//		#include <ctype.h>
//		#include <stdlib.h>
//		#include <math.h>
		
		private const bool IS_DEBUG = false;
		
		private const int NUM_LAB = 100;
		private const int LAB_LEN = 10;
		private const int FOR_NEST = 25;
		private const int SUB_NEST = 25;
		private const int PROG_SIZE = 10000;
		
		//token_type
		private const byte DELIMITER = 1;
		private const byte VARIABLE = 2;
		private const byte NUMBER = 3;
		private const byte COMMAND = 4;
		private const byte STRING = 5;
		private const byte QUOTE = 6;
		
		//tok, for token_type COMMAND
		private const byte PRINT = 1;
		private const byte INPUT = 2;
		private const byte IF = 3;
		private const byte THEN = 4;
		private const byte FOR = 5;
		private const byte NEXT = 6;
		private const byte TO = 7;
		private const byte GOTO = 8;
		private const byte EOL = 9;
		private const byte FINISHED = 10;
		private const byte GOSUB = 11;
		private const byte RETURN = 12;
		private const byte END = 13;
		
		private static CharPtr prog;  /* holds expression to be analyzed */
		private static jmp_buf e_buf = new jmp_buf(); /* hold environment for longjmp() */
		
		private static double[] variables = new double[26]{    /* 26 user variables,  A-Z */
		  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		  0, 0, 0, 0, 0, 0
		};
		
		private class commands { /* keyword lookup table */
			public CharPtr command = new CharPtr(new char[20]);
		  	public byte tok;
		  	
		  	public commands(CharPtr command, byte tok) {
		  		this.command = new CharPtr(command);
		  		this.tok = tok;
		  	}
		}
		private static commands[] table = { /* Commands must be entered lowercase */
			new commands("print", PRINT), /* in this table. */
			new commands("input", INPUT),
			new commands("if", IF),
			new commands("then", THEN),
			new commands("goto", GOTO),
			new commands("for", FOR),
			new commands("next", NEXT),
			new commands("to", TO),
			new commands("gosub", GOSUB),
			new commands("return", RETURN),
			new commands("end", END),
			new commands("", END)  /* mark end of table */
		};
		
		private static CharPtr token = new CharPtr(new char[80]);
		
		public class label {
			public CharPtr name = new CharPtr(new char[LAB_LEN]);
		  	public CharPtr p;  /* points to place to go in source file*/
		};
		public static label[] label_table = label_table_init();
		private static label[] label_table_init() {
			label[] result = new label[NUM_LAB];
			for (int i = 0; i < result.Length; ++i)
			{
				result[i] = new label();
			}
			return result;
		}
		
//		char *find_label(), *gpop();
		
		public class for_stack {
		  	public int var; /* counter variable */
		  	public double target;  /* target value */
		  	public CharPtr loc;
		  	
		  	public for_stack() {}
		  	public for_stack(for_stack copy) {
		  		this.var = copy.var;
		  		this.target = copy.target;
		  		this.loc = new CharPtr(copy.loc);
		  	}
		}
		private static for_stack[] fstack = fstack_init(); /* stack for FOR/NEXT loop */
		private static for_stack[] fstack_init() {
			for_stack[] result = new for_stack[FOR_NEST];
			for (int i = 0; i < result.Length; ++i)
			{
				result[i] = new for_stack();
			}
			return result;
			
		}
//		struct for_stack fpop();
		
		private static CharPtr[] gstack = new CharPtr[SUB_NEST];	/* stack for gosub */
		
		private static int ftos;  /* index to top of FOR stack */
		private static int gtos;  /* index to top of GOSUB stack */
		
//		void assignment();
//		void print(), scan_labels(), find_eol(), exec_goto();
//		void exec_if(), exec_for(), next(), fpush(), input();
//		void gosub(), greturn(), gpush(), label_init();
//		void serror(), get_exp(), putback();
//		void level2(), level3(), level4(), level5(), level6(), primitive();
//		void unary(), arith();
//		int load_program(char *p, char *fname), look_up(char *s);
//		int get_next_label(char *s), iswhite(char c), isdelim(char c);
//		double find_var(char *s);
//		void get_token(char *token_type, char *tok);
		
		public static int main(int argc, CharPtr[] argv)
		{
		  	CharPtr p_buf;
		  	byte token_type = 0;
		  	byte tok = 0;
		  
		  	if (argc != 2) {
		    	printf("usage: tinybasic <filename>\n");
		    	exit(1);
		  	}
		
		  	/* allocate memory for the program */
		  	if (null == (p_buf = (CharPtr) malloc(PROG_SIZE))) {
		    	printf("allocation failure");
		    	exit(1);
		  	}
		  
		  	/* load the program to execute */
		  	if (0 == load_program(new CharPtr(p_buf), argv[1])) exit(1);
		  
		  	//if (0 != setjmp(e_buf)) exit(1); /* initialize the long jump buffer */
		  
		  	try {
			  	prog = new CharPtr(p_buf);
			  	scan_labels(); /* find the labels in the program */
			  
			  	ftos = 0; /* initialize the FOR stack index */
			  	gtos = 0; /* initialize the GOSUB stack index */
			  	do {
			    
			    	get_token(ref token_type, ref tok);
			    	/* check for assignment statement */
			    	if (token_type == VARIABLE) {
			      		putback(); /* return the var to the input stream */
			      		assignment(); /* must be assignment statement */
			    	}
			    	else /* is command */
			    		switch ((int)tok) {
			        	case PRINT:
				        	print();
			  	      		break;
			        
			  	      	case GOTO:
			          		exec_goto();
			          		break;
			        
			          	case IF:
			          		exec_if();
			          		break;
			        
			          	case FOR:
			          		exec_for();
			          		break;
			        
			          	case NEXT:
			          		next();
			          		break;
			        
			          	case INPUT:
			          		input();
			          		break;
			        
			          	case GOSUB:
			          		gosub();
			          		break;
			        
			          	case RETURN:
			          		greturn();
			          		break;
			        
			          	case END:
			          		exit(0);
			          		break;
			      		}
			  	} while (tok != FINISHED);
		  	} 
		  	catch (LongjmpException e)
		  	{
		  		Debug.WriteLine(e.ToString());
		  		//if (0 != setjmp(e_buf)) exit(1); /* initialize the long jump buffer */
		  		exit(1); /* initialize the long jump buffer */
		  	}
			return 0;
		}
		
		/* Load a program. */
		private static int load_program(CharPtr p, CharPtr fname)
		{
		  	FILE fp;
		  	int i = 0;
		
		  	if (null == (fp = fopen(fname, "rb"))) return 0;
		
		  	i = 0;
		  	do {
		  		p[0] = (char)getc(fp);
		  		p.inc();
		    	i++;
		  	} while(0 == feof(fp) && i < PROG_SIZE);
		  	if (false) {
		  		p[-2] = '\0'; /* null terminate the program */
		  	} else {
		  		//FIXME:
		  		p[0] = '\0'; /* null terminate the program */
		  	}
		  	fclose(fp);
		  	return 1;
		}
		
		/* Assign a variable a value. */
		private static void assignment()
		{
		  	int var;
		  	double value_ = 0;
		  	byte token_type = 0;
		  	byte tok = 0;
		  
		  	/* get the variable name */
		  	get_token(ref token_type, ref tok);
		  	if (0 == isalpha(token[0])) {
		    	serror(4);
		  	}
		
		  	var = toupper(token[0]) - 'A';
		 
		  	/* get the equals sign */
		  	get_token(ref token_type, ref tok);
		  	if (token[0] != '=') {
		    	serror(3);
		  	}
		  
		  	/* get the value to assign to var */
		  	get_exp(ref value_);
		  	/* assign the value */
		  	variables[var] = value_;
		}
		
		private static int print_t = 0;
		/* Execute a simple version of the BASIC PRINT statement */
		private static void print()
		{
		  	double answer = 0;
		  	int len = 0, spaces;
		  	char last_delim = (char)0;
		  	byte token_type = 0;
		  	byte tok = 0;
		
		  	if (IS_DEBUG) {
	    		printf("\n[print_t: %d]\n", print_t);
	    		print_t++;
		  	}
	  	
		  	
		  	int kkk = 0;
		  	do {
		    	get_token(ref token_type, ref tok); /* get next list item */
		    	
		    	if (IS_DEBUG) {
		    		printf("\n[print: %d, EOL: %d, kkk: %d]\n", tok, EOL, kkk);
		    		kkk++;
		    	}
		    	
		    	if (tok == EOL || tok == FINISHED) break;
		    	if (token_type == QUOTE) { /* is string */
		    		printf("%s", new CharPtr(token).ToString());
		      		len += (int)strlen(token);
		      		get_token(ref token_type, ref tok);
		    	}
		    	else { /* is expression */
		      		putback();
		      		get_exp(ref answer);
		      		get_token(ref token_type, ref tok);
		      		if ( (long)floor(answer * 1000.0) % 1000 < 10 )
		      			len += printf("%.0f", answer);
		      		else
		      			len += printf("%.4f", answer);
		    	}
		    
		    	last_delim = token[0];
		    
		    	if (token[0] == ';') {
		      		/* compute number of spaces to move to next tab */
		      		spaces = 8 - (len % 8);
		      		len += spaces; /* add in the tabbing position */
		      		while (0 != spaces) {
			      		printf(" ");
		        		spaces--;
		      		}
		    	}
		    	else if (token[0] == ',')  { 
		    		/* do nothing */; 
		    	}
		    	else if (tok != EOL && tok != FINISHED) {
		    		serror(0);
		    	}
		    	
		    //FIXME:	
		  	//} while (token[0] != '\r' || token[0] == ';' || token[0] == ',');
			} while (token[0] == ';' || token[0] == ',');
		  	
		  	if (tok == EOL || tok == FINISHED) {
		    	if (last_delim != ';' && last_delim != ',') printf("\n");
		  	}
		  	else {
		  		if (false) {
		  			
		  		} else {
		  			serror(0); /* error is not , or ; */
		  		}
		  	}
		  	
		  	if (IS_DEBUG) {
	    		printf("\n[print_t:out: %d]\n", print_t);
	    		print_t++;
		  	}
		}
		
		/* Find all labels. */
		private static void scan_labels()
		{
		  	int addr;
		  	CharPtr temp;
		  	byte token_type = 0;
		  	byte tok = 0;
		  
		  	label_init();  /* zero all labels */
		  	temp = new CharPtr(prog);   /* save pointer to top of program */
		  
		  	/* if the first token in the file is a label */
		  	get_token(ref token_type, ref tok);
		  
		  	if (token_type == NUMBER) {
		    	strcpy(label_table[0].name, token);
		    	label_table[0].p = new CharPtr(prog);
		  	}
		  
		  	find_eol();
		  	do {
		    	get_token(ref token_type, ref tok);
		    	if (token_type == NUMBER) {
		    		addr = get_next_label(new CharPtr(token));
		      		if(addr == -1 || addr == -2) {
		      			if (addr == -1) { serror(5); } else { serror(6); }
		      		}
		      
		      		strcpy(label_table[addr].name, token);
		      		label_table[addr].p = new CharPtr(prog);  /* current point in program */
		    	}
		    	/* if not on a blank line, find next line */
		    	if (tok != EOL) find_eol();
		  	} while(tok != FINISHED);
		  
		  	prog = new CharPtr(temp);  /* restore to original */
		}
		
		/* Find the start of the next line. */
		private static void find_eol()
		{
			while (prog[0] != '\n' && prog[0] != '\0') {
				prog.inc();
		  	}
		  	//if(*prog) prog++;
		}
		
		/* Return index of next free position in label array.
		   A -1 is returned if the array is full.
		   A -2 is returned when duplicate label is found.
		*/
		private static int get_next_label(CharPtr s)
		{
		  	int t;
		
		  	for (t = 0; t < NUM_LAB; ++t) {
		    	if (label_table[t].name[0] == 0) return t;
		    	if (0 == strcmp(label_table[t].name, s)) return -2; /* dup */
		  	}
		
		  	return -1;
		}
		
		/* Find location of given label.  A null is returned if
		   label is not found; otherwise a pointer to the position
		   of the label is returned.
		*/
		private static CharPtr find_label(CharPtr s)
//		char *s;
		{
		  	int t;
		
		  	if (IS_DEBUG) {
	  			printf("find_label <<< %s\n", s.ToString());
	  		}
		  	for (t = 0; t < NUM_LAB; ++t) {
		  		if (IS_DEBUG) {
		  			printf("find_label >>> %s\n", new CharPtr(label_table[t].name).ToString());
		  		}
		  		if (0 == strcmp(label_table[t].name, s)) {
		  			return new CharPtr(label_table[t].p);
		  		}
		  	}
		  	return new CharPtr(""); /* error condition */
		}
		
		/* Execute a GOTO statement. */
		private static void exec_goto()
		{
		  	CharPtr loc;
		  	byte token_type = 0;
		  	byte tok = 0;
		
		  	get_token(ref token_type, ref tok); /* get label to go to */
		  	/* find the location of the label */
		  	loc = find_label(new CharPtr(token));
		  	if (loc[0] == '\0')
		    	serror(7); /* label not defined */
		
		  	else prog = loc;  /* start program running at that loc */
		}
		
		/* Initialize the array that holds the labels.
		   By convention, a null label name indicates that
		   array position is unused.
		*/
		private static void label_init()
		{
		  	int t;
		
		  	for (t = 0; t < NUM_LAB; ++t) label_table[t].name[0] = '\0';
		}
		
		/* Execute an IF statement. */
		private static void exec_if()
		{
		  	double x = 0, y = 0;
		  	int cond;
		  	CharPtr op = new CharPtr(new char[3]);
		  	op[0] = '\0';
		  	byte token_type = 0;
		  	byte tok = 0;
		
		  	get_exp(ref x); /* get left expression */
		  	get_token(ref token_type, ref tok); /* get the operator */
		  	if (strcmp("<>", token) != 0 && null == strchr("=<>", token[0])) {
		    	serror(0); /* not a legal operator */
		    	return;
		  	}
		  
		  	strcpy(op, token);
		  
		  	get_exp(ref y); /* get right expression */
		  	/* determine the outcome */
		  	cond = 0;
		
		 	if(strcmp("<>", op) == 0) {
		    	if (x != y) cond = 1;
		  	} else if (null != strchr("<", op[0])) {
		    	if (x < y) cond = 1;
		  	} else if (null != strchr(">", op[0])) {
		    	if (x > y) cond = 1;
		  	} else if (null != strchr("=", op[0])) {
		    	if (x == y) cond = 1;
		  	}
		  	if (0 != cond) { /* is true so process target of IF */
		    	get_token(ref token_type, ref tok);
		    	if (tok != THEN) {
		      		serror(8);
		      		return;
		    	}/* else program execution starts on next line */
		    	if (false) {
		    		exec_goto(); //<-- added line!
		    	}
		  	}
		  	else {
		    	find_eol(); /* find start of next line */
		  	}
		}
		
		/* Execute a FOR loop. */
		private static void exec_for()
		{
			for_stack i = new for_stack();
		  	double value_ = 0;
		  	byte token_type = 0;
		  	byte tok = 0;
		
		  	get_token(ref token_type, ref tok); /* read the control variable */
		  	if (0 == isalpha(token[0])) {
		    	serror(4);
		    	return;
		  	}
		
		  	i.var = toupper(token[0]) - 'A'; /* save its index */
		
		  	get_token(ref token_type, ref tok); /* read the equals sign */
		  	if (token[0] != '=') {
		    	serror(3);
		    	return;
		  	}
		
		  	get_exp(ref value_); /* get initial value */
		
		  	variables[i.var] = value_;
		
		  	get_token(ref token_type, ref tok);
		  	if(tok != TO) serror(9); /* read and discard the TO */
		
		  	get_exp(ref i.target); /* get target value */
		
		  	/* if loop can execute at least once, push info on stack */
		  	if (value_ >= variables[i.var]) {
		   		i.loc = prog;
		    	fpush(i);
		  	}
		  	else  /* otherwise, skip loop code altogether */
		    	while(tok != NEXT) get_token(ref token_type, ref tok);
		}
		
		/* Execute a NEXT statement. */
		private static void next()
		{
			for_stack i = new for_stack();
		
			i = new for_stack(fpop()); /* read the loop info */
		
		  	variables[i.var]++; /* increment control variable */
		  	if (variables[i.var] > i.target) return;  /* all done */
		  	fpush(i);  /* otherwise, restore the info */
		  	prog = i.loc;  /* loop */
		}
		
		/* Push function for the FOR stack. */
		private static void fpush(for_stack i)
//		struct for_stack i;
		{
			if (ftos > FOR_NEST)
		    	serror(10);
		
			fstack[ftos] = new for_stack(i);
		  	ftos++;
		}
		
		private static for_stack fpop()
		{
		  	ftos--;
		  	if (ftos < 0) serror(11);
		  	return new for_stack(fstack[ftos]);
		}
		
		/* Execute a simple form of the BASIC INPUT command */
		private static void input()
		{
		  	char var;
		  	object[] i = {0};
		  	byte token_type = 0;
		  	byte tok = 0;
		
		  	get_token(ref token_type, ref tok); /* see if prompt string is present */
		  	if (token_type == QUOTE) {
		  		printf("%s", new CharPtr(token).ToString()); /* if so, print it and check for comma */
		    	get_token(ref token_type, ref tok);
		    	if (token[0] != ',') serror(1);
		    	get_token(ref token_type, ref tok);
		  	}
		  	else printf("? "); /* otherwise, prompt with / */
		  	var = (char)(toupper(token[0]) - 'A'); /* get the input var */
		
		  	scanf("%d", i); /* read input */
		
		  	variables[(int)var] = (int)(i[0]); /* store it */
		}
		
		/* Execute a GOSUB command. */
		private static void gosub()
		{
		  	CharPtr loc;
		  	byte token_type = 0;
		  	byte tok = 0;
		
		  	get_token(ref token_type, ref tok);
		  	/* find the label to call */
		  	loc = find_label(new CharPtr(token));
		  	if (loc[0] == '\0')
		    	serror(7); /* label not defined */
		  	else {
		    	gpush(prog); /* save place to return to */
		    	prog = new CharPtr(loc);  /* start program running at that loc */
		  	}
		}
		
		/* Return from GOSUB. */
		private static void greturn()
		{
			prog = gpop();
		}
		
		/* GOSUB stack push function. */
		private static void gpush(CharPtr s)
//		char *s;
		{
		  	gtos++;
		
		  	if (gtos == SUB_NEST) {
		    	serror(12);
		    	return;
		  	}
		
		  	gstack[gtos] = new CharPtr(s);
		
		}
		
		/* GOSUB stack pop function. */
		private static CharPtr gpop()
		{
		  	if (gtos == 0) {
		    	serror(13);
		    	return null;
		  	}
		
		  	return new CharPtr(gstack[gtos--]);
		}
		
		/* Entry point into parser. */
		private static void get_exp(ref double result)
//		double *result;
		{
		  	byte token_type = 0;
		  	byte tok = 0;
		  	get_token(ref token_type, ref tok);
		  	if (0 == token[0]) {
		    	serror(2);
		    	return;
		  	}
		  	level2(ref result, token_type, tok);
		  	putback(); /* return last token read to input stream */
		}
		
		
		private static CharPtr[] serror_e = {
		    	"syntax error",
		    	"unbalanced parentheses",
		    	"no expression present",
		    	"equals sign expected",
		    	"not a variable",
		    	"Label table full",
		    	"duplicate label",
		    	"undefined label",
		    	"THEN expected",
		    	"TO expected",
		    	"too many nested FOR loops",
		    	"NEXT without FOR",
		    	"too many nested GOSUBs",
		    	"RETURN without GOSUB"
		  	};
		/* display an error message */
		private static void serror(int error)
//		int error;
		{
			printf("%s\n", serror_e[error].ToString());
		  	longjmp(e_buf, 1); /* return to save point */
		}
		
		/* Get a token. */
		private static void get_token(ref byte token_type, ref byte tok)
		{
		
		  	CharPtr temp;
		
		  	temp = new CharPtr(token);
		
		  	if (true) {
		  		//FIXME: why???
		  		token_type = 0;
		  		tok = 0;
		  	}
		  	
		  	//FIXME:
		  	if (IS_DEBUG) {
		  		printf("\n[get_token: %s]\n", new CharPtr(token).ToString());
		  	}
		  	
		 	if (prog[0] == '\0') { /* end of file */
		 		token[0] = (char)0;
		    	tok = FINISHED;
		    	token_type = DELIMITER;
		    	return;
		  	}
		
		 	while (0 != iswhite(prog[0])) prog.inc();  /* skip over white space */
		  
		  
		 	if (prog[0] == '\r') { /* crlf */
		 		prog.inc(); prog.inc();
		 		tok = EOL; token[0] = '\r';
		 		token[1] = '\n'; token[2] = (char)0;
		 		token_type = DELIMITER;
		    	return;
		  	} 
		 	else if (prog[0] == '\n') {
		 		prog.inc();
		 		tok = EOL;
		 		token[0] = '\n';
		 		token[1] = (char)0;
		    	token_type = DELIMITER;
		    	return;
		  	}
		  
		  	// if clause delimiter x <> y
		  	if ((prog[0] == '<') && (prog[+1] == '>')) {
		  		temp[0] = prog[0];
		    	prog.inc();
		    	temp.inc();
		    	temp[0] = prog[0];
		    	prog.inc();
		    	temp.inc();
		    	temp[0] = (char)0;
		    	token_type = DELIMITER;
		    	return;
		  	}
		  
		  	if (null != strchr("+-*^/%=;(),><", prog[0])){ /* delimiter */
		  		temp[0] = prog[0];
		  		prog.inc(); /* advance to next position */
		  		temp.inc();
		  		temp[0] = (char)0;
		  		token_type = DELIMITER;
		    	return;
		  	}
		  
		  	if (prog[0] == '"') { /* quoted string */
		  		prog.inc();
		  		while (prog[0] != '"' && (prog[0] != '\r' || prog[0] != '\n')) {
		  			temp[0] = prog[0];
		  			prog.inc();
		  			temp.inc();
		  		}
		    	if (prog[0] == '\r' || prog[0] == '\n') serror(1);
		    	prog.inc(); temp[0] = (char)0;
		    	token_type = QUOTE;
		    	return;
		  	}
		  
		  	if (0 != isdigit(prog[0])) { /* number */
		  		while (0 == isdelim(prog[0])) {
		  			temp[0] = prog[0];
		  			prog.inc();
		  			temp.inc();
		    	}
		  		temp[0] = '\0';
		  		token_type = NUMBER;
		    	return;
		  	}
		  
		  	if(0 != isalpha(prog[0])) { /* var or command */
		  		while(0 == isdelim(prog[0])) {
		  			temp[0] = prog[0];
		  			prog.inc();
		  			temp.inc();
		  		}
		  		token_type = STRING;
		  	}
		
		  	temp[0] = '\0';
		  
		  	/* see if a string is a command or a variable */
		  	if (token_type == STRING) {
		  		tok = look_up(token); /* convert to internal rep */
		  		if (0 == tok) token_type = VARIABLE;
		  		else token_type = COMMAND; /* is a command */
		  	}
		}
		
		
		
		/* Return a token to input stream. */
		private static void putback()
		{
			CharPtr t;
		  	t = new CharPtr(token);
		  	for(; t[0] != 0; t.inc()) prog.dec();
		}
		
		/* Look up a a token's internal representation in the
		   token table.
		*/
		private static byte look_up(CharPtr s)
		{
		  	int i;
		  	CharPtr p;
		
		  	if (IS_DEBUG) {
		  		Console.WriteLine("[>>>>" + s.ToString() + "<<<<]");
		  	}
		  	
		  	/* convert to lowercase */
		  	p = new CharPtr(s);
		  	while (p[0] != 0) { p[0] = tolower(p[0]); p.inc(); }
		
		  	/* see if token is in table */
		  	for (i = 0; table[i].command[0] != 0; i++) {
		  		if (0 == strcmp(table[i].command, new CharPtr(s))) {
			  		return table[i].tok;
			  	}
		  	}
		  	return 0; /* unknown command */
		}
		
		/* Return true if c is a delimiter. */
		private static int isdelim(char c)
		{
		  	if (null != strchr(" ;,+-<>/*%^=()", c) || c == 9 || c == '\r' || c == '\n' || c == 0)
		    	return 1;
		  	return 0;
		}
		
		/* Return 1 if c is space or tab. */
		private static int iswhite(char c)
		{
			if (c == ' ' || c == '\t') return 1;
		  	else return 0;
		}
		
		
		
		/*  Add or subtract two terms. */
		private static void level2(ref double result, byte token_type, byte tok)
//		double *result;
//		char token_type;
//		char tok;
		{
		  	char op;
		  	double hold = 0;
		
		  	level3(ref result, token_type, tok);
		  	while ((op = token[0]) == '+' || op == '-') {
		    	get_token(ref token_type, ref tok);
		    	level3(ref hold, token_type, tok);
		    	arith(op, ref result, ref hold);
		  	}
		}
		
		/* Multiply or divide two factors. */
		private static void level3(ref double result, byte token_type, byte tok)
//		double *result;
//		char token_type;
//		char tok;
		{
		  	char  op;
		  	double hold = 0;
		
		  	level4(ref result, token_type, tok);
		  	while ((op = token[0]) == '*' || op == '/' || op == '%') {
		    	get_token(ref token_type, ref tok);
		    	level4(ref hold, token_type, tok);
		    	arith(op, ref result, ref hold);
		  	}
		}
		
		/* Process integer exponent. */
		private static void level4(ref double result, byte token_type, byte tok)
//		double *result;
//		char token_type;
//		char tok;
		{
		  	double hold = 0;
		
		  	level5(ref result, token_type, tok);
		  	if (token[0] == '^') {
		    	get_token(ref token_type, ref tok);
		    	level4(ref hold, token_type, tok);
		    	arith('^', ref result, ref hold);
		  	}
		}
		
		/* Is a unary + or -. */
		private static void level5(ref double result, byte token_type, byte tok)
//		double *result;
//		char token_type;
//		char tok;
		{
		  	char op;
		
		  	op = (char)0;
		  	if ((token_type == DELIMITER) && (token[0] == '+' || token[0] == '-')) {
		  		op = token[0];
		    	get_token(ref token_type, ref tok);
		  	}
		  	level6(ref result, token_type, tok);
		  	if (op != 0)
		    	unary(op, ref result);
		}
		
		/* Process parenthesized expression. */
		private static void level6(ref double result, byte token_type, byte tok)
//		double *result;
//		char token_type;
//		char tok;
		{
			if ((token[0] == '(') && (token_type == DELIMITER)) {
				get_token(ref token_type, ref tok);
		    	level2(ref result, token_type, tok);
		    	if(token[0] != ')')
		      		serror(1);
		    	get_token(ref token_type, ref tok);
		  	}
		  	else
		    	primitive(ref result, token_type, tok);
		}
		
		/* Find value of number or variable. */
		private static void primitive(ref double result, byte token_type, byte tok)
//		double *result;
//		char token_type;
//		char tok;
		{
		
			switch (token_type) {
		  	case VARIABLE:
				result = find_var(token);
				get_token(ref token_type, ref tok);
		    	return;
		  
		    case NUMBER:
		    	result = atof(token);
		    	get_token(ref token_type, ref tok);
		    	return;
		  	
		    default:
		    	serror(0);
		    	break;
		  }
		}
		
		/* Perform the specified arithmetic. */
		private static void arith(char o, ref double r, ref double h)
//		char o;
//		double *r, *h;
		{
		  	int t, ex;
		
		  	switch (o) {
		    case '-':
		  		r = r - h;
		      	break;
		      
		    case '+':
		      	r = r + h;
		      	break;
		      
		    case '*':
		      	r = r * h;
		      	break;
		      
		    case '/':
		      	r = r / h;
		      	break;
		      
		    case '%':
		      	t = (int)((r) / (h));
		      	r = r - (t * (h));
		      	break;
		      
		    case '^':
		      	ex = (int)r;
		      	if (h == 0) {
		      		r = 1;
		        	break;
		      	}
		      	for (t = (int)(h - 1); t > 0; --t) r = (r) * ex;
		      	break;
		  	}
		}
		
		/* Reverse the sign. */
		private static void unary(char o, ref double r)
//		char o;
//		double *r;
		{
			if (o == '-') r = -(r);
		}
		
		/* Find the value of a variable. */
		private static double find_var(CharPtr s)
		{
			if (0 == isalpha(s[0])) {
		    	serror(4); /* not a variable */
		    	return 0;
		  	}
			return variables[toupper(token[0]) - 'A'];
		}
	}
}
