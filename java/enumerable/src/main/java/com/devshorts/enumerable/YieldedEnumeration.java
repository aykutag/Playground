package com.devshorts.enumerable;

import com.devshorts.enumerable.data.Yieldable;
import com.devshorts.enumerable.iterators.YieldedEnumerationIterator;

import java.util.Iterator;
import java.util.function.Supplier;

public class YieldedEnumeration<TSource> implements Iterable<TSource>  {

    private Supplier<Yieldable<TSource>> generator;

    public YieldedEnumeration(Supplier<Yieldable<TSource>> generator) {
        super();

        this.generator = generator;
    }

    @Override
    public Iterator<TSource> iterator() {
        return new YieldedEnumerationIterator<>(generator);
    }
}
