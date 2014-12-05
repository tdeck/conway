( Conway's Game of Life in gforth )

variable grid-rows
variable grid-cols

( *** Game Logic *** )

: grid-size ( -- )   grid-rows @ grid-cols @ * ;

: allot-grid ( -- ) ( allots a grid and fills it with zeros )
    here grid-size dup allot erase ;

( Pointers to grids, stored with continguous rows )
variable 'this-grid
variable 'next-grid

( Create two grids that we can swap between )
: setup-grids ( rows cols -- )
    grid-cols ! grid-rows !
    here 'this-grid ! allot-grid
    here 'next-grid ! allot-grid ;

: swap-grids ( -- )   'this-grid @ 'next-grid @ 'this-grid ! 'next-grid ! ;

: rc>index ( r c -- i ) swap grid-cols @ * + ;

: alive? ( r c -- b )
    2dup rc>index -rot
    ( bounds checking first )
    dup 0 < swap grid-cols @ >= or
    swap 
    dup 0 < swap grid-rows @ >= or
    or if
        drop false
    else
        'this-grid @ + C@ 
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
: set-this-state ( r c b -- )   'this-grid @ set-state ;
: set-next-state ( r c b -- )   'next-grid @ set-state ;

: compute-new-grid ( -- )
    grid-rows @ 0 do
        grid-cols @ 0 do
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

: show-living ( -- )    2b24 xemit space ;
: show-dead ( -- )   25cc xemit space ;
decimal

: show-grid ( -- )
    page
    bright inverse
    grid-rows @ 0 do
        grid-cols @ 0 do
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
: gliders 0 do 0 i 5 * glider loop ;

250 constant frame-delay

: simulate ( -- )   begin show-grid step frame-delay ms 0 until ;

init-game 
form 10 / gliders drop
simulate

