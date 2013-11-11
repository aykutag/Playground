package com.devshorts.enumerable;

import java.util.Iterator;
import java.util.function.Predicate;

public class FilterEnumerable<TSource> extends Enumerable<TSource, TSource>{

    private int idx = 0;
    private TSource nextItem = null;
    private Predicate<TSource> filterFunc;

    public FilterEnumerable(Iterator<TSource> input, Predicate<TSource> filterFunc) {
        super(input);
        this.filterFunc = filterFunc;
    }

    @Override
    public boolean hasNext() {
        while(source.hasNext()){
            nextItem = source.next();
            idx++;
            if(filterFunc.test(nextItem)){
                return true;
            }
        }

        return false;
    }

    @Override
    public TSource next() {
        return nextItem;
    }
}
