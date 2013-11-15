package com.devshorts.enumerable.iterators;

import com.devshorts.enumerable.data.IndexValuePair;
import com.devshorts.enumerable.iterators.DefaultEnumIterator;

import java.util.function.Consumer;

public class IterEnumerable<TSource> extends DefaultEnumIterator<TSource> {

    private int idx = 0;
    private Consumer<IndexValuePair<TSource>> action;

    public IterEnumerable(Iterable<TSource> source, Consumer<IndexValuePair<TSource>> action) {
        super(source);
        this.action = action;
    }

    @Override
    public TSource next(){
        TSource n = (TSource)source.next();
        action.accept(new IndexValuePair<>(n, idx));
        idx++;
        return n;
    }

    @Override protected void reset(){
        super.reset();

        idx = 0;
    }
}
