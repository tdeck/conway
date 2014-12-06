( Conway's Game of Life in gforth )

250 constant frame-delay
-28 constant SIGTERM

10 value grid-rows
10 value grid-cols

( *** Game Logic *** )

: grid-size ( -- )   grid-rows grid-cols * ;

: allot-grid ( -- ) ( allots a grid and fills it with zeros )
    here grid-size dup allot erase ;

( Pointers to grids, stored with continguous rows )
0 value 'this-grid
0 value 'next-grid

( Create two grids that we can swap between )
: setup-grids ( rows cols -- )
    to grid-cols to grid-rows
    here to 'this-grid allot-grid
    here to 'next-grid allot-grid ;

: swap-grids ( -- )   'this-grid 'next-grid to 'this-grid to 'next-grid ;

: rc>index ( r c -- i ) swap grid-cols * + ;

: alive? ( r c -- b )
    2dup rc>index -rot
    ( bounds checking first )
    dup 0 < swap grid-cols >= or
    swap 
    dup 0 < swap grid-rows >= or
    or if
        drop false
    else
        'this-grid + C@ 
    then ;    

: alive# ( r c -- 0/1 )   alive? 1 and ;

: count-l-r-neighbors ( r c -- n )
    2dup 1- alive# -rot
    2dup alive# -rot
    1+ alive# + + ;

: row-above ( r c -- r c )   swap 1- swap ;
: row-below ( r c -- r c )   swap 1+ swap ;

: count-neighbors ( r c -- n )
    2dup alive# negate -rot
    ( upper row ) 2dup row-above count-l-r-neighbors -rot
    ( center row ) 2dup count-l-r-neighbors -rot
    ( lower row) row-below count-l-r-neighbors 
    + + + ;

: will-live? ( r c -- b )
    2dup count-neighbors -rot alive?
    if ( original cell is alive )
        dup 2 = swap 3 = or
    else ( original cell is dead )
        3 = 
    then ;

: set-state ( r c b grid -- )   2swap rc>index + c! ;
: set-this-state ( r c b -- )   'this-grid set-state ;
: set-next-state ( r c b -- )   'next-grid set-state ;

: compute-new-grid ( -- )
    grid-rows 0 do
        grid-cols 0 do
            j i 2dup will-live? set-next-state
        loop
    loop ;

: step ( -- ) compute-new-grid swap-grids ;

( *** I/O *** )

: init-game ( -- )  form 2 / swap 2 - swap setup-grids ;

hex
: CSI ( -- )   1b emit ." [" ;
: normal ( -- )   CSI ." 0m" ;
: bright ( -- )   CSI ." 1m" ;
: white ( -- )   CSI ." 37m" ;
: green ( -- )   CSI ." 32m" ;
: inverse ( -- )   CSI ." 7m" ;

: hide-cursor ( -- )   CSI ." ?25l" ;

: show-living ( -- )    2b24 xemit space ;
: show-dead ( -- )   25cc xemit space ;
decimal

: show-grid ( -- )
    0 0 at-xy
    bright white inverse
    grid-rows 0 do
        grid-cols 0 do
            j i alive? if show-living else show-dead then
        loop
        cr
    loop ;

: glider ( r c -- ) ( TODO - not the cleanest way to write this)
    ( row 0 )
    2dup false set-this-state
    2dup 1+ true set-this-state
    2dup 2 + false set-this-state
    ( row 1 )
    row-below
    2dup false set-this-state
    2dup 1+ false set-this-state
    2dup 2 + true set-this-state
    ( row 2 )
    row-below
    2dup true set-this-state
    2dup 1 + true set-this-state
    2 + true set-this-state ;

: gliders ( n -- )   0 do 0 i 5 * glider loop ;

: light-square   ( r c -- ) 
    2dup
    2 * swap at-xy green 
    alive? if show-living else show-dead then ;

: at-last-line ( -- )   0 form drop at-xy ;

: confine-coords ( -- ) 
    0 max grid-cols 1- min ( column )
    swap
    0 max grid-rows 1- min ( row )
    swap ;

: populate ( -- )   
    at-last-line ." Use h,j,k,l to move, SPACE to set, ENTER when done."
    hide-cursor
    0 0
    begin
        confine-coords
        show-grid
        2dup light-square
        key
            dup 104 = if ( 'h' )
                -rot 1- rot
            else dup 106 = if ( 'j' )
                -rot row-below rot
            else dup 107 = if ( 'k' )
                -rot row-above rot
            else dup 108 = if ( 'l' )
                -rot 1+ rot
            else dup 32 = if ( space )
                -rot 2dup 2dup alive? 0= set-this-state rot
            then then then then then
        13 =
    until 
    normal
    at-last-line form swap drop 0 do space loop ;


: simulate ( -- )   
    at-last-line ." Hit ^C to exit."
    begin 
        show-grid 
        step 
        frame-delay ms 
    0 until ;

: game ( -- )   
    page
    init-game 
    ['] populate catch dup 0= if
        ['] simulate catch 
    then 
    normal dup SIGTERM = if
        cr bye
    else 
        throw
    then ;

game
