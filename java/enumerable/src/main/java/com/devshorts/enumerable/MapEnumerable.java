package com.devshorts.enumerable;

import java.util.Iterator;
import java.util.function.Function;
import java.util.function.Supplier;

/**
 * Created with IntelliJ IDEA.
 * User: anton.kropp
 * Date: 11/11/13
 * Time: 5:11 PM
 * To change this template use File | Settings | File Templates.
 */
public class MapEnumerable<TSource, TResult> extends Enumerable<TResult> {

    protected Iterator<TSource> source;
    protected Supplier<Iterator<TSource>> generator;
    private Function<TSource, TResult> predicate;


    public MapEnumerable(Iterable<TSource> input){
        generator = () -> input.iterator();

        source = generator.get();
    }

    public MapEnumerable(Iterable<TSource> source, Function<TSource, TResult> map) {
        this(source);

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

    @Override
    protected void reset(){
        source = generator.get();
    }

    @Override
    public Iterator<TResult> iterator(){
        reset();

        return this;
    }
}
