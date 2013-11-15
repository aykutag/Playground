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

    private <T> Enumerable<T> enumerableWithIterator(Supplier<Iterator<T>> generator){
        return new Enumerable<>(this, generator);
    }

    protected Enumerable(Iterable source, Supplier<Iterator<TSource>> iteratorGenerator) {
        this.source = source;
        this.iteratorGenerator = iteratorGenerator;
    }

    public <TResult> Enumerable<TResult> map(Function<TSource, TResult> mapFunc){
        return enumerableWithIterator(() -> new MapEnumerable<>(this, i -> mapFunc.apply(i)));
    }

    public <TResult> Enumerable<TResult> flatMap(Function<TSource, List<TResult>> mapFunc){
        return enumerableWithIterator(() -> new FlatMapEnumerable<>(this, i -> mapFunc.apply(i)));
    }

    public Enumerable<TSource> filter(Predicate<TSource> filterFunc){
        return enumerableWithIterator(() -> new FilterEnumerable<>(this, filterFunc));
    }

    public Enumerable<TSource> take(int n){
        return enumerableWithIterator(() -> new TakeEnumerable<>(this, n));
    }

    public Enumerable<TSource> takeWhile(Predicate<TSource> predicate){
        return enumerableWithIterator(() -> new TakeWhileEnumerable<>(this, predicate));
    }

    public Enumerable<TSource> skip(int skipNum){
        return enumerableWithIterator(() -> new SkipEnumerable<>(this, skipNum));
    }

    public Enumerable<TSource> skipWhile(Predicate<TSource> predicate){
        return enumerableWithIterator(() -> new SkipWhile<>(this, predicate));
    }

    public Enumerable<TSource> iter(Consumer<TSource> action){
        return enumerableWithIterator(() -> new IterEnumerable<>(this, idxPair -> action.accept(idxPair.value)));
    }

    public Enumerable<TSource> iteri(BiConsumer<Integer, TSource> action){
        return enumerableWithIterator(() -> new IterEnumerable<>(this, idxPair -> action.accept(idxPair.index, idxPair.value)));
    }

    public <TProjection> Enumerable<TSource> orderBy(Function<TSource, TProjection> projection){
        return enumerableWithIterator(() -> new OrderByEnumerable(this, projection));
    }

    public <TSecond, TProjection> Enumerable<TProjection> zip(Iterable<TSecond> zipWith, BiFunction<TSource, TSecond, TProjection> zipper){
        return enumerableWithIterator(() -> new ZipEnumerable<>(this, zipWith, zipper));
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

