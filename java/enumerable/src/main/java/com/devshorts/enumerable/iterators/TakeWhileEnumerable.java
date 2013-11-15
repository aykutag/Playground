package com.devshorts.enumerable.iterators;

import com.devshorts.enumerable.iterators.DefaultEnumIterator;

import java.util.Iterator;
import java.util.function.Predicate;

/**
 * Created with IntelliJ IDEA.
 * User: anton.kropp
 * Date: 11/11/13
 * Time: 4:54 PM
 * To change this template use File | Settings | File Templates.
 */
public class TakeWhileEnumerable<TSource> extends DefaultEnumIterator<TSource> {
    private Predicate<TSource> predicate;
    private TSource nextItem;

    public TakeWhileEnumerable(Iterable<TSource> results, Predicate<TSource> predicate) {
        super(results);
        this.predicate = predicate;
    }

    @Override
    public boolean hasNext(){
        if(source.hasNext()){
            nextItem = (TSource)source.next();

            return predicate.test(nextItem);
        }

        return false;
    }

    @Override
    public TSource next(){
        return nextItem;
    }
}
