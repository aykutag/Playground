package com.devshorts.enumerable;

import java.util.function.Function;

public class MapEnumerable<TSource, TResult> extends Enumerable<TResult> {

    private Function<TSource, TResult> projection;


    /***
     * Need this constructor for flatMap
     * @param input
     */
    public MapEnumerable(Iterable<TSource> input){
        super(() -> input.iterator());

        // by default the projection is the id function
        this.projection = i -> (TResult)i;
    }

    public MapEnumerable(Iterable<TSource> source, Function<TSource, TResult> projection) {
        this(source);

        this.projection = projection;
    }

    @Override
    public boolean hasNext() {
        return source.hasNext();
    }

    @Override
    public TResult next() {
        return projection.apply((TSource)source.next());
    }
}
