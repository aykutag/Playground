package com.devshorts.enumerable;

import com.devshorts.enumerable.iterators.*;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.function.*;

public class Enumerable<TSource> implements Iterable<TSource> {

    private Iterable source;
    private Supplier<Iterator<TSource>> iteratorGenerator;

    public static <TSource> Enumerable<TSource> init(Iterable<TSource> source){
        return new Enumerable<TSource>(source, () -> new DefaultEnumIterator<>(source));
    }

    private <T> Enumerable<T> builder(Supplier<Iterator<T>> generator){
        return new Enumerable<>(this, generator);
    }

    protected Enumerable(Iterable source, Supplier<Iterator<TSource>> iteratorGenerator) {
        this.source = source;
        this.iteratorGenerator = iteratorGenerator;
    }

    public <TResult> Enumerable<TResult> map(Function<TSource, TResult> mapFunc){
        return builder(() -> new MapEnumerable<>(this, i -> mapFunc.apply(i)));
    }

    public <TResult> Enumerable<TResult> flatMap(Function<TSource, List<TResult>> mapFunc){
        return builder(() -> new FlatMapEnumerable<>(this, i -> mapFunc.apply(i)));
    }

    public Enumerable<TSource> filter(Predicate<TSource> filterFunc){
        return builder(() -> new FilterEnumerable<>(this, filterFunc));
    }

    public Enumerable<TSource> take(int n){
        return builder(() -> new TakeEnumerable<>(this, n));
    }

    public Enumerable<TSource> takeWhile(Predicate<TSource> predicate){
        return builder(() -> new TakeWhileEnumerable<>(this, predicate));
    }

    public Enumerable<TSource> skip(int skipNum){
        return builder(() -> new SkipEnumerable<>(this, skipNum));
    }

    public Enumerable<TSource> iter(Consumer<TSource> action){
        return builder(() -> new IterEnumerable<>(this, idxPair -> action.accept(idxPair.value)));
    }

    public Enumerable<TSource> iteri(BiConsumer<Integer, TSource> action){
        return builder(() -> new IterEnumerable<>(this, idxPair -> action.accept(idxPair.index, idxPair.value)));
    }

    public <TProjection> Enumerable<TSource> orderBy(Function<TSource, TProjection> projection){
        return builder(() -> new OrderByEnumerable(this, projection));
    }

    public <TSecond, TProjection> Enumerable<TProjection> zip(Iterable<TSecond> zipWith, BiFunction<TSource, TSecond, TProjection> zipper){
        return builder(() -> new ZipEnumerable<>(this, zipWith, zipper));
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

    @Override
    public Iterator<TSource> iterator() {
        return iteratorGenerator.get();
    }
}

