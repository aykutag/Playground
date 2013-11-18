package com.devshorts.enumerable.iterators;

import java.util.HashSet;

public class DistinctIterator<TSource> extends EnumerableIterator<TSource> {

    private HashSet<TSource> set = new HashSet<>();

    private TSource last;

    public DistinctIterator(Iterable<TSource> input) {
        super(input);
    }

    @Override
    public boolean hasNext(){
        while(source.hasNext()){
            last = source.next();

            if(!set.contains(last)){
                set.add(last);
                return true;
            }
        }

        return false;
    }

    @Override
    public TSource next(){
        return last;
    }
}
