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
    dup 0 < swap grid-rows @ >= or
    swap 
    dup 0 < swap grid-cols @ >= or
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

: set-next-state ( r c b -- )   -rot rc>index 'next-grid @ + c! ;

: compute-new-grid ( -- )
    grid-rows @ 0 do
        grid-cols @ 0 do
            j i 2dup will-live? set-next-state
        loop
    loop ;

: step ( -- ) compute-new-grid swap-grids ;

( *** I/O *** )

: init-game ( -- )  form swap 2 - swap setup-grids ;

hex
: show-living ( -- )   25a3 xemit ;
: show-dead ( -- )   25a1 xemit ;
decimal

: show-grid ( -- )
    page
    grid-rows @ 0 do
        grid-cols @ 0 do
            j i alive? if show-living else show-dead then
        loop
        cr
    loop ;

: glider ( -- ) ( TODO - not the cleanest way to write this)
    ( row 0 )
    0 0 false set-next-state
    0 1 true set-next-state
    0 2 false set-next-state
    ( row 1 )
    1 0 false set-next-state
    1 1 false set-next-state
    1 2 true set-next-state
    ( row 2 )
    2 0 true set-next-state
    2 1 true set-next-state
    2 2 true set-next-state 
    swap-grids ;

100 constant frame-delay

: simulate ( -- )   begin show-grid step frame-delay ms 0 until ;

init-game glider simulate

