package com.devshorts.enumerable.iterators;

import com.devshorts.enumerable.iterators.DefaultEnumIterator;

import java.util.Iterator;
import java.util.LinkedList;
import java.util.Queue;
import java.util.Stack;
import java.util.function.Predicate;

/**
 * Created with IntelliJ IDEA.
 * User: anton.kropp
 * Date: 11/11/13
 * Time: 4:59 PM
 * To change this template use File | Settings | File Templates.
 */
public class SkipWhile<TSource> extends DefaultEnumIterator<TSource> {
    private Predicate<TSource> predicate;

    private Queue<TSource> nextItems = new LinkedList<>();



    public SkipWhile(Iterable<TSource> source, Predicate<TSource> predicate){
        super(source);
        this.predicate = predicate;

        skip();
    }

    private void skip() {
        while(source.hasNext()){
            TSource item = source.next();

            if(predicate.test(item)){
                continue;
            }

            nextItems.add(item);

            return;
        }
    }

    @Override
    public boolean hasNext(){
        if(source.hasNext()){
            nextItems.add(source.next());
        }

        return nextItems.peek() != null;
    }

    @Override
    public TSource next(){
        return nextItems.remove();
    }
}
