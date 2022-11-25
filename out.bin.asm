bits 16 ; 16 Bit Real Mode
call davis_function_Main

; Struct `i16` (Packed)
; Fields:
; - Name: low
;   Type: i8
;   Size: 1
;
; - Name: high
;   Type: i8
;   Size: 1
;
; Total Size: 2

; Davis function Main
; Returns: i8
; Arguments:
davis_function_Main:
  ; Variable `i8 one` declared here, not assigned a value.
  sub sp, 2
  ; pretend one is i16;
  ; ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
  ;    Explicit unsafe cast occurred here.
  ; Begin inline assembly.

  ; fuck you, no more i8
	
  ; End inline assembly.
  ; Stack cleanup.
  add sp, 2
  ; Implicit return from function at end due to no 'return' keyword.
  ret
; End of Davis function Main
