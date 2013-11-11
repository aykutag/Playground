package com.devshorts.enumerable;

import java.util.Iterator;

/**
 * Created with IntelliJ IDEA.
 * User: anton.kropp
 * Date: 11/11/13
 * Time: 6:51 PM
 * To change this template use File | Settings | File Templates.
 */
public class ResettableIterator<T> {

    private Iterator<T> iterator;
    private Iterable<T> iterable;

    public ResettableIterator(Iterable<T> iterable){
        this.iterable = iterable;
        iterator = iterable.iterator();
    }
    public Iterator<T> get(){
        return iterator;
    }

    public void reset(){
        iterator = iterable.iterator();
    }
}
