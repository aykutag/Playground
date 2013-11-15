package com.devshorts.enumerable.iterators;

import com.devshorts.enumerable.Enumerable;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.function.Function;

public class OrderByIterator<TSource> extends EnumerableIterator<TSource> {
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

    public OrderByIterator(Iterable<TSource> source, Function<TSource, ? extends Comparable> projection) {
        super(source);

        this.projection = projection;

        sort();
    }

    @Override
    public boolean hasNext(){
        return idx < buffer.size();
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
