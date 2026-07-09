       IDENTIFICATION DIVISION.
       PROGRAM-ID. ALFACLI.

       ENVIRONMENT                             DIVISION.
       INPUT-OUTPUT                        SECTION.
       FILE-CONTROL.
           SELECT REQUEST-FILE ASSIGN TO "io/request.txt"
               ORGANIZATION IS LINE SEQUENTIAL
               FILE STATUS IS WS-REG-STATUS.

           SELECT REPOSNSE-FILE ASSIGN TO "io/response.txt"
               ORGANIZATION IS LINE SEQUENTIAL 
               FILE STATUS IS WS-RESP-STATUS.

           SELECT CLIENT-FILE ASSIGN TO "data/clientes.dat"
               ORGANIZATION IS LINE SEQUENTIAL
               FILE STATUS IS WS-CLI-STATUS.

       DATA                                    DIVISION.
       FILE                                SECTION. 

       FD REQUEST-FILE.
       01 REQUEST-RECORD.
           05 REQ-ID        PIC 9(10).
       01 REQ-OPERACAO PIC X(10).
               88 OP-CONSULTAR VALUE "CONSULTAR ".
               88 OP-ATUALIZAR VALUE "ATUALIZAR ".
           05 REQ-TELEFONE  PIC X(11).
           05 REQ-EMAIL     PIC X(80).

       FD RESPONSE-FILE.
       01 REPONSE-RECORD    PIC X(250).

       FD CLIENT-FILE.
       01 CLIENT-RECORD     PIC X(250).


       WORKING-STORAGE                     SECTION.

       01 WS-REQ-STATUS  PIC XX VALUE SPACES.
       01 WS-RESP-STATUS PIC XX VALUE SPACES.
       01 WS-CLI-STATUS  PIC XX VALUE SPACES.

       01 WS-EOF         PIC X VALUE "N".
           88 EOF        PIC X VALUE "Y".
           88 NOT-EOF    PIC X VALUE "N".
       
       01 WS-FOUND         PIC X VALUE "N".
           88 FOUND        PIC X VALUE "Y".
           88 NOT-FOUND    PIC X VALUE "N".

       01 WS-SAVE-STATUS   PIC X VALUE "S".
           88 SAVED        PIC X VALUE "s".
           88 NOT-SAVED    PIC X VALUE "F".


       01 WS-IDX       PIC 9(03) VALUE 0.
       01 WS-COUNT     PIC 9(03) VALUE 0.
       01 WS-FOUND-IDX PIC 9(03) VALUE 0.
       01 WS-AT-COUNT  PIC 9(03) VALUE 0.

       01 WS-CLIENTES.
           05 TB-CLIENTE OCCURS 100 TIMES.
               10 TB-ID       PIC X(06).
               10 TB-NOME     PIC X(60).
               10 TB-TELEFONE PIC X(11).
               10 TB-EMAIL    PIC X(80).

       01 WS-RESPONSE.
           05 RESP-RETORNO  PIC X(04) VALUE SPACES.
           05 RESP-MENSAGEM PIC X(80) VALUE SPACES.
           05 RESP-ID   PIC X(06) VALUE SPACES.
           05 RESP-NOME     PIC X(60) VALUE SPACES.
           05 RESP-TELEFONE PIC X(11) VALUE SPACES.
           05 RESP-EMAIL    PIC X(80) VALUE SPACES.

       PROCEDURE                                   DIVISION.

       MAIN-PROCEDURE.
           PERFORM READ-REQUEST

           IF RESP-RETORNO = SPACES
               EVALUATE TRUE
                   WHEN OP-CONSULTAR
                       PERFORM CONSULTAR-CLIENTE

                   WHEN OP-ATUALIZAR
                       PERFORM ATUALIZAR-CLIENTE

                   WHEN OTHER 
                       MOVE "0422" TO RESP-RETORNO
                       MOVE "OPERACAO INVALIDA" TO RESP-MENSAGEM
               END-EVALUATE
           END-IF

           PERFORM WRITE-RESPONSE

           STOP RUN.

       READ-REQUEST.
           OPEN INPUT REQUEST-FILE

           IF WS-REQ-STATUS NOT = "00"
               MOVE "0500" TO RESP-RETORNO
               MOVE "Arquivo de request nao encontrado." TO RESP-MENSAGEM
           ELSE
               READ REQUEST-FILE
                   AT END
                       MOVE "0500" TO RESP-RETORNO
                       MOVE "Arquivo de request vazio." TO RESP-MENSAGEM
                   NOT AT END
                       CONTINUE
               END-READ

               CLOSE REQUEST-FILE
           END-IF.

       LOAD-CLIENTES.
           MOVE 0 TO WS-COUNT
           SET NOT-EOF TO TRUE

           OPEN INPUT CLIENTES-FILE

           IF WS-CLI-STATUS NOT = "00"
               MOVE "0500" TO RESP-RETORNO
               MOVE "Arquivo de clientes nao encontrado." TO RESP-MENSAGEM
           ELSE
               PERFORM UNTIL EOF OR WS-COUNT >= 100
                   READ CLIENTES-FILE
                       AT END
                           SET EOF TO TRUE
                       NOT AT END
                           ADD 1 TO WS-COUNT
                           MOVE SPACES TO TB-CLIENTE(WS-COUNT)

                           UNSTRING CLIENTE-LINE DELIMITED BY "|"
                               INTO TB-CODIGO(WS-COUNT)
                                    TB-NOME(WS-COUNT)
                                    TB-TELEFONE(WS-COUNT)
                                    TB-EMAIL(WS-COUNT)
                           END-UNSTRING
                   END-READ
               END-PERFORM

               CLOSE CLIENTES-FILE
           END-IF.

       FIND-CLIENTE.
           SET CLIENTE-NOT-FOUND TO TRUE
           MOVE 0 TO WS-FOUND-IDX

           PERFORM VARYING WS-IDX FROM 1 BY 1
               UNTIL WS-IDX > WS-COUNT OR CLIENTE-FOUND

               IF TB-CODIGO(WS-IDX) = REQ-CODIGO
                   SET CLIENTE-FOUND TO TRUE
                   MOVE WS-IDX TO WS-FOUND-IDX
               END-IF

           END-PERFORM.

       CONSULTAR-CLIENTE.
           PERFORM FIND-CLIENTE

           IF CLIENTE-FOUND
               MOVE "0000" TO RESP-RETORNO
               MOVE "Cliente encontrado." TO RESP-MENSAGEM
               PERFORM MOVE-CLIENTE-TO-RESPONSE
           ELSE
               MOVE "0404" TO RESP-RETORNO
               MOVE "Cliente nao encontrado." TO RESP-MENSAGEM
               MOVE REQ-CODIGO TO RESP-CODIGO
           END-IF.

       ATUALIZAR-CLIENTE.
           PERFORM VALIDAR-CONTATO

           IF RESP-RETORNO = SPACES
               PERFORM FIND-CLIENTE

               IF CLIENTE-FOUND
                   MOVE REQ-TELEFONE TO TB-TELEFONE(WS-FOUND-IDX)
                   MOVE REQ-EMAIL    TO TB-EMAIL(WS-FOUND-IDX)

                   PERFORM SAVE-CLIENTES

                   IF SAVE-OK
                       MOVE "0000" TO RESP-RETORNO
                       MOVE "Contato atualizado com sucesso."
                           TO RESP-MENSAGEM
                       PERFORM MOVE-CLIENTE-TO-RESPONSE
                   END-IF
               ELSE
                   MOVE "0404" TO RESP-RETORNO
                   MOVE "Cliente nao encontrado." TO RESP-MENSAGEM
                   MOVE REQ-CODIGO TO RESP-CODIGO
               END-IF
           END-IF.

       VALIDAR-CONTATO.
           IF REQ-TELEFONE = SPACES
               MOVE "0422" TO RESP-RETORNO
               MOVE "Telefone obrigatorio." TO RESP-MENSAGEM
           END-IF

           IF RESP-RETORNO = SPACES
               IF REQ-EMAIL = SPACES
                   MOVE "0422" TO RESP-RETORNO
                   MOVE "Email obrigatorio." TO RESP-MENSAGEM
               END-IF
           END-IF

           IF RESP-RETORNO = SPACES
               MOVE 0 TO WS-AT-COUNT
               INSPECT REQ-EMAIL TALLYING WS-AT-COUNT FOR ALL "@"

               IF WS-AT-COUNT = 0
                   MOVE "0422" TO RESP-RETORNO
                   MOVE "Email invalido." TO RESP-MENSAGEM
               END-IF
           END-IF.

       SAVE-CLIENTES.
           SET SAVE-OK TO TRUE

           OPEN OUTPUT CLIENTES-FILE

           IF WS-CLI-STATUS NOT = "00"
               SET SAVE-FAILED TO TRUE
               MOVE "0500" TO RESP-RETORNO
               MOVE "Erro ao salvar arquivo de clientes."
                   TO RESP-MENSAGEM
           ELSE
               PERFORM VARYING WS-IDX FROM 1 BY 1
                   UNTIL WS-IDX > WS-COUNT

                   MOVE SPACES TO CLIENTE-LINE

                   STRING
                       FUNCTION TRIM(TB-CODIGO(WS-IDX))
                           DELIMITED BY SIZE
                       "|" DELIMITED BY SIZE
                       FUNCTION TRIM(TB-NOME(WS-IDX))
                           DELIMITED BY SIZE
                       "|" DELIMITED BY SIZE
                       FUNCTION TRIM(TB-TELEFONE(WS-IDX))
                           DELIMITED BY SIZE
                       "|" DELIMITED BY SIZE
                       FUNCTION TRIM(TB-EMAIL(WS-IDX))
                           DELIMITED BY SIZE
                       INTO CLIENTE-LINE
                   END-STRING

                   WRITE CLIENTE-LINE
               END-PERFORM

               CLOSE CLIENTES-FILE
           END-IF.

       MOVE-CLIENTE-TO-RESPONSE.
           MOVE TB-CODIGO(WS-FOUND-IDX)   TO RESP-CODIGO
           MOVE TB-NOME(WS-FOUND-IDX)     TO RESP-NOME
           MOVE TB-TELEFONE(WS-FOUND-IDX) TO RESP-TELEFONE
           MOVE TB-EMAIL(WS-FOUND-IDX)    TO RESP-EMAIL.

       WRITE-RESPONSE.
           OPEN OUTPUT RESPONSE-FILE

           IF WS-RESP-STATUS = "00"
               MOVE SPACES TO RESPONSE-RECORD

               STRING
                   FUNCTION TRIM(RESP-RETORNO)
                       DELIMITED BY SIZE
                   "|" DELIMITED BY SIZE
                   FUNCTION TRIM(RESP-MENSAGEM)
                       DELIMITED BY SIZE
                   "|" DELIMITED BY SIZE
                   FUNCTION TRIM(RESP-CODIGO)
                       DELIMITED BY SIZE
                   "|" DELIMITED BY SIZE
                   FUNCTION TRIM(RESP-NOME)
                       DELIMITED BY SIZE
                   "|" DELIMITED BY SIZE
                   FUNCTION TRIM(RESP-TELEFONE)
                       DELIMITED BY SIZE
                   "|" DELIMITED BY SIZE
                   FUNCTION TRIM(RESP-EMAIL)
                       DELIMITED BY SIZE
                   INTO RESPONSE-RECORD
               END-STRING

               WRITE RESPONSE-RECORD

               CLOSE RESPONSE-FILE
           ELSE
               DISPLAY "ERRO AO CRIAR RESPONSE.TXT"
           END-IF.
