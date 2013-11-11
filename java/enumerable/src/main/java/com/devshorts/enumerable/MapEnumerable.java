package com.devshorts.enumerable;

import java.util.function.Function;

/**
 * Created with IntelliJ IDEA.
 * User: anton.kropp
 * Date: 11/11/13
 * Time: 5:11 PM
 * To change this template use File | Settings | File Templates.
 */
public class MapEnumerable<TSource, TResult> extends Enumerable<TSource, TResult> {

    private final Function<TSource, TResult> predicate;

    public MapEnumerable(Iterable<TSource> source, Function<TSource, TResult> map) {
        super(source);
        this.predicate = map;
    }

    @Override
    public boolean hasNext() {
        return source.hasNext();
    }

    @Override
    public TResult next() {
        if(predicate == null){
            return (TResult)source.next();
        }

        return predicate.apply(source.next());
    }
}
