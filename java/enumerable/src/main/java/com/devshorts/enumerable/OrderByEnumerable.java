package com.devshorts.enumerable;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Iterator;
import java.util.List;
import java.util.function.Function;

public class OrderByEnumerable<TSource> extends Enumerable<TSource> {
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
    }

    @Override
    public boolean hasNext(){
        Boolean hasNext = idx < buffer.size();
        if(!hasNext){
            buffer = null;
        }

        return hasNext;
    }

    private List<TSource> list(){
        List<TSource> r = new ArrayList<>();
        while(super.hasNext()){
            r.add(super.next());
        }
        return r;
    }

    @Override
    public TSource next(){
        TSource value = (TSource)buffer.get(idx).value;
        idx++;
        return value;
    }

    @Override
    public Iterator<TSource> iterator(){
        reset();

        if(buffer == null || idx >= buffer.size()){
            idx = 0;

            buffer = Enumerable.init(list())
                    .map(value -> new ProjectionPair(projection.apply(value), value))
                    .toList();

            Collections.sort(buffer);
        }

        return this;
    }
}
