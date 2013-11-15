package com.devshorts.enumerable.iterators;

import com.devshorts.enumerable.Enumerable;
import com.devshorts.enumerable.iterators.DefaultEnumIterator;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Iterator;
import java.util.List;
import java.util.function.Function;

public class OrderByEnumerable<TSource> extends DefaultEnumIterator<TSource> {
    private class ProjectionPair<T extends Comparable, Y> implements Comparable<T>{
        public T projection;
        public Y value;
        public ProjectionPair(T projection, Y value){
            this.projection = projection;
            this.value = value;
        }

        @Override
        public int compareTo(T o) {
            return o.compareTo(projection);
        }
    }

    private List<ProjectionPair> buffer;
    private Function<TSource, ? extends Comparable> projection;
    private Integer idx = 0;

    public OrderByEnumerable(Iterable<TSource> source, Function<TSource, ? extends Comparable> projection) {
        super(source);

        this.projection = projection;

        sort();
    }

    @Override
    public boolean hasNext(){
        Boolean hasNext = idx < buffer.size();
        if(!hasNext){
            buffer = null;
        }

        return hasNext;
    }

    @Override
    public TSource next(){
        TSource value = (TSource)buffer.get(idx).value;
        idx++;
        return value;
    }

    private void sort(){
        idx = 0;

        buffer = Enumerable.init(evaluateEnumerable())
                .map(value -> new ProjectionPair(projection.apply(value), value))
                .toList();

        Collections.sort(buffer);
    }

    private List<TSource> evaluateEnumerable(){
        List<TSource> r = new ArrayList<>();
        while(super.hasNext()){
            r.add(super.next());
        }
        return r;
    }

}
