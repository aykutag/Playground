package com.devshorts.enumerable;

import java.util.Iterator;
import java.util.function.Predicate;

/**
 * Created with IntelliJ IDEA.
 * User: anton.kropp
 * Date: 11/11/13
 * Time: 4:54 PM
 * To change this template use File | Settings | File Templates.
 */
public class TakeWhileEnumerable<TSource> extends Enumerable<TSource, TSource> {
    private Predicate<TSource> predicate;
    private TSource nextItem;

    public TakeWhileEnumerable(Iterator<TSource> results, Predicate<TSource> predicate) {
        super(results);
        this.predicate = predicate;
    }

    @Override
    public boolean hasNext(){
        if(source.hasNext()){
            nextItem = source.next();

            return predicate.test(nextItem);
        }

        return false;
    }

    @Override
    public TSource next(){
        return nextItem;
    }
}
