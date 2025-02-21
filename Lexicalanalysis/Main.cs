#include <stdio.h>
#include <stdlib.h>
#include <ctype.h>
#include <string.h>

#define MAX_LENGTH 100

int isKeywordOrDatatype(char str[]) {
    char *keywords[] = {"for", "while", "do", "if", "else", "switch", "case", "return"};
    char *datatypes[] = {"int", "float", "char", "double", "void", "long", "short", "static"};
    int i;
    
    for (i = 0; i < 8; i++) {
        if (strcmp(str, keywords[i]) == 0) {
            return 1; 
        }
    }
    
    for (i = 0; i < 8; i++) {
        if (strcmp(str, datatypes[i]) == 0) {
            return 2; 
        }
    }
    
    return 0; 
}

int isOperator(char c) {
    char operators[] = "+-*/%=<>!&|";
    for (int i = 0; i < strlen(operators); i++) {
        if (c == operators[i])
            return 1;
    }
    return 0;
}

int isHeaderFile(char str[]) {
    return strstr(str, "#include") != NULL;
}

int main() {
    FILE *source, *fIdentifiers, *fOperators, *fNumbers, *fDataTypes, *fHeaders;
    char c, token[MAX_LENGTH];
    int index = 0, lineNumber = 1;
    
    
    source = fopen("lexicalanalyzer.txt", "r");
    fIdentifiers = fopen("identifiers.txt", "w");
    fOperators = fopen("operators.txt", "w");
    fNumbers = fopen("numbers.txt", "w");
    fDataTypes = fopen("datatypes.txt", "w");
    fHeaders = fopen("headers.txt", "w");

    if (!source) {
        printf("Error opening source file.\n");
        return 1;
    }

    while ((c = fgetc(source)) != EOF) {
        
        if (isdigit(c)) {
            token[index++] = c;
            c = fgetc(source);
            while (isdigit(c) || c == '.') {
                token[index++] = c;
                c = fgetc(source);
            }
            token[index] = '\0';
            fprintf(fNumbers, "%s\n", token);
            index = 0;
            ungetc(c, source);
        }

     
        else if (isalpha(c) || c == '_') {
            token[index++] = c;
            c = fgetc(source);
            while (isalnum(c) || c == '_') {
                token[index++] = c;
                c = fgetc(source);
            }
            token[index] = '\0';
            int type = isKeywordOrDatatype(token);
            if (type == 1)
                fprintf(fDataTypes, "%s (Keyword)\n", token);
            else if (type == 2)
                fprintf(fDataTypes, "%s (DataType)\n", token);
            else
                fprintf(fIdentifiers, "%s\n", token);
            index = 0;
            ungetc(c, source);
        }

        else if (isOperator(c)) {
            fputc(c, fOperators);
            c = fgetc(source);
            if (isOperator(c)) {
                fputc(c, fOperators); 
            } else {
                ungetc(c, source);
            }
            fputc('\n', fOperators);
        }

        else if (c == '#') {
            token[index++] = c;
            c = fgetc(source);
            while (c != '\n' && c != EOF) {
                token[index++] = c;
                c = fgetc(source);
            }
            token[index] = '\0';
            if (isHeaderFile(token))
                fprintf(fHeaders, "%s\n", token);
            index = 0;
        }

        else if (c == '\n') {
            lineNumber++;
        }
    }

    fclose(source);
    fclose(fIdentifiers);
    fclose(fOperators);
    fclose(fNumbers);
    fclose(fDataTypes);
    fclose(fHeaders);

    printf("Lexical analysis completed. Check output files.\n");
    return 0;
}
