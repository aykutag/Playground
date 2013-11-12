package com.devshorts.enumerable;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.function.*;

public class Enumerable<TSource> implements Iterator<TSource>, Iterable<TSource> {

    protected Iterator source;
    protected Supplier<Iterator> generator;

    public static <TSource> Enumerable<TSource> init(Iterable<TSource> source){
        return new Enumerable<>(source);
    }

    protected Enumerable(Supplier<Iterator> generator){
        this.generator = generator;
    }

    public Enumerable(Iterable<TSource> input) {
        this((Supplier<Iterator>) input::iterator);
    }

    public <TResult> Enumerable<TResult> map(Function<TSource, TResult> mapFunc){
        return new MapEnumerable<>(this, i -> mapFunc.apply(i));
    }

    public <TResult> Enumerable<TResult> flatMap(Function<TSource, List<TResult>> mapFunc){
        return new FlatMapEnumerable<>(this, i -> mapFunc.apply(i));
    }

    public Enumerable<TSource> filter(Predicate<TSource> filterFunc){
        return new FilterEnumerable<>(this, filterFunc);
    }

    public Enumerable<TSource> take(int n){
        return new TakeEnumerable<>(this, n);
    }

    public Enumerable<TSource> takeWhile(Predicate<TSource> predicate){
        return new TakeWhileEnumerable<>(this, predicate);
    }

    public Enumerable<TSource> skip(int skipNum){
        return new SkipEnumerable<>(this, skipNum);
    }

    public Enumerable<TSource> iter(Consumer<TSource> action){
        return new IterEnumerable<>(this, idxPair -> action.accept(idxPair.value));
    }

    public Enumerable<TSource> iteri(BiConsumer<Integer, TSource> action){
        return new IterEnumerable<>(this, idxPair -> action.accept(idxPair.index, idxPair.value));
    }

    public <TProjection> Enumerable<TSource> orderBy(Function<TSource, TProjection> projection){
        return new OrderByEnumerable(this, projection);
    }

    public List<TSource> toList(){
        List<TSource> r = new ArrayList<>();

        for(TSource item : this){
            r.add(item);
        }

        return r;
    }

    /**
     * Iterator methods
     */

    protected void reset(){
        source = generator.get();
    }

    @Override
    public Iterator<TSource> iterator() {
        reset();

        return this;
    }

    @Override
    public boolean hasNext() {
        return source.hasNext();
    }

    @Override
    public TSource next() {
        return (TSource)source.next();
    }
}
