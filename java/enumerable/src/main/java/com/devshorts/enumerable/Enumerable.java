package com.devshorts.enumerable;

import com.devshorts.enumerable.data.Action;
import com.devshorts.enumerable.data.Yieldable;
import com.devshorts.enumerable.iterators.*;

import java.util.*;
import java.util.function.*;

class YieldedEnumeration<TSource> implements Iterable<TSource>  {

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

public class Enumerable<TSource> implements Iterable<TSource> {

    //private Iterable source;

    private Supplier<Iterator<TSource>> iteratorGenerator;

    public static <TSource> Enumerable<TSource> init(Iterable<TSource> source){
        return new Enumerable<>(() -> new EnumerableIterator<>(source));
    }

    public static <TSource> Enumerable<TSource> generate(Supplier<Yieldable<TSource>> generator){
        return new Enumerable<>(() -> new EnumerableIterator<>(new YieldedEnumeration<>(generator)));
    }

    public static <TSource> Enumerable<TSource> generate(Supplier<Yieldable<TSource>> generator,
                                                         Action onNewIterator){
        return new Enumerable<>(() -> {
            onNewIterator.exec();

            return new EnumerableIterator<>(new YieldedEnumeration<>(generator));
        });
    }

    private <T> Enumerable<T> enumerableWithIterator(Supplier<Iterator<T>> generator){
        return new Enumerable<>(generator);
    }

    protected Enumerable(Supplier<Iterator<TSource>> iteratorGenerator) {
        this.iteratorGenerator = iteratorGenerator;
    }

    public <TResult> Enumerable<TResult> map(Function<TSource, TResult> mapFunc){
        return enumerableWithIterator(() -> new MapIterator<>(this, i -> mapFunc.apply(i)));
    }

    public <TResult> Enumerable<TResult> flatMap(Function<TSource, List<TResult>> mapFunc){
        return enumerableWithIterator(() -> new FlatMapIterator<>(this, i -> mapFunc.apply(i)));
    }

    public Enumerable<TSource> filter(Predicate<TSource> filterFunc){
        return enumerableWithIterator(() -> new FilterIterator<>(this, filterFunc));
    }

    public Enumerable<TSource> take(int n){
        return enumerableWithIterator(() -> new TakeIterator<>(this, n));
    }

    public Enumerable<TSource> takeWhile(Predicate<TSource> predicate){
        return enumerableWithIterator(() -> new TakeWhileIterator<>(this, predicate));
    }

    public Enumerable<TSource> skip(int skipNum){
        return enumerableWithIterator(() -> new SkipIterator<>(this, skipNum));
    }

    public Enumerable<TSource> skipWhile(Predicate<TSource> predicate){
        return enumerableWithIterator(() -> new SkipWhileIterator<>(this, predicate));
    }

    public Enumerable<TSource> iter(Consumer<TSource> action){
        return enumerableWithIterator(() ->
                new IndexIterator<>(this, idxPair -> action.accept(idxPair.value)));
    }

    public Enumerable<TSource> iteri(BiConsumer<Integer, TSource> action){
        return enumerableWithIterator(() ->
                new IndexIterator<>(this, idxPair -> action.accept(idxPair.index, idxPair.value)));
    }

    public <TProjection> Enumerable<TSource> orderBy(Function<TSource, Comparable<TProjection>> projection){
        return orderBy(projection, (o1, o2) -> o1.compareTo((TProjection) o2));
    }

    public <TProjection> Enumerable<TSource> orderByDesc(Function<TSource, Comparable<TProjection>> projection){
        return orderBy(projection, (o1, o2) -> o2.compareTo((TProjection) o1));
    }

    public <TProjection> Enumerable<TSource> orderBy(Function<TSource, Comparable<TProjection>> projection,
                                                     Comparator<Comparable<TProjection>> comparator){
        return enumerableWithIterator(() ->new OrderByIterator(this, projection, comparator));
    }

    public <TSecond, TProjection> Enumerable<TProjection> zip(Iterable<TSecond> zipWith,
                                                              BiFunction<TSource, TSecond, TProjection> zipper){
        return enumerableWithIterator(() -> new ZipIterator<>(this, zipWith, zipper));
    }

    public TSource first(){
        return unsafeIterEval(new NthIterator<>(this, 1));
    }

    public TSource firstOrDefault(){
        return orDefault(new NthIterator<>(this, 1));
    }

    public TSource nth(int n){
        return unsafeIterEval(new NthIterator<>(this, n));
    }

    public TSource nthOrDefault(int n){
        return orDefault(new NthIterator<>(this, n));
    }

    public TSource last(){
        return unsafeIterEval(new LastIterator<>(this));
    }

    public TSource lastOrDefault(){
        return orDefault(new LastIterator<>(this));
    }

    public <TAcc> TAcc fold(BiFunction<TAcc, TSource, TAcc> accumulator, TAcc seed){
        return evalUnsafeMapIterator(new FoldIterator<>(this, accumulator, seed));
    }


    /**
     * Folds using the first element as the seed
     * @param accumulator
     * @return
     */
    public TSource foldWithFirst(BiFunction<TSource, TSource, TSource> accumulator){
        return unsafeIterEval(new FoldWithDefaultSeedIterator<>(this, accumulator));
    }

    public Boolean any(Predicate<TSource> predicate){
        return evalUnsafeMapIterator(new PredicateIterator<>(
                this,
                predicate,
                (acc, elem) -> acc || elem,
                i -> i,
                () -> true,
                false
        ));
    }

    public Boolean all(Predicate<TSource> predicate){
        return evalUnsafeMapIterator(new PredicateIterator<>(
                this,
                predicate,
                (acc, elem) -> acc && elem,
                i -> !i,
                () -> false,
                true
        ));
    }

    public List<TSource> toList(){
        List<TSource> r = new LinkedList<>();

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


    private <TAcc> TAcc evalUnsafeMapIterator(Iterator<TAcc> iterator) {
        iterator.hasNext();

        return iterator.next();
    }

    private TSource unsafeIterEval(Iterator<TSource> iterator) {
        iterator.hasNext();

        return iterator.next();
    }

    private TSource orDefault(Iterator<TSource> iterator){
        if(iterator.hasNext()){
            return iterator.next();
        }

        return null;
    }
}

