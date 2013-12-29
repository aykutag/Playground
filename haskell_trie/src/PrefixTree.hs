module PrefixTree (insert, exists, empty, Trie(Node)) where

import qualified Data.List as L
import Data.Maybe
import Control.Monad
import qualified Data.Foldable as Fold

type Key a = [a]

data Trie key = Node (Maybe key) [Trie key] Bool deriving (Show, Eq, Read)

empty :: [Trie key]
empty = [Node Nothing [] False]

findKey :: (Eq t) => t -> [Trie t] -> Maybe (Trie t)
findKey key tries = L.find (\(Node next _ _) -> next == Just key) tries

findTrie :: (Eq t) => Key t -> [Trie t] -> Maybe (Trie t)
findTrie [] _ = Nothing
findTrie (x:[]) tries = findKey x tries 
findTrie (x:xs) tries = findKey x tries >>= nextTrie
    where nextTrie (Node _ next _) = findTrie xs next

exists :: (Eq t) => Key t -> [Trie t] -> Maybe Bool
exists keys trie = findTrie keys trie >>= \(Node _ _ isWord) -> 
    if isWord then return isWord 
    else Nothing

exists (x:xs) tries = findKey x tries >>= nextTrie
    where nextTrie (Node _ next _) = exists xs next
                
insert :: (Eq t) => Key t -> [Trie t] -> [Trie t]
insert [] _ = []
insert (x:xs) tries = 
    case findKey x tries of 
        Nothing -> [(Node (Just x) (insert xs [])) endWord]++tries
        Just value -> 
            let (Node key next word) = value
            in [Node key (insert xs next) word]++(except value)
    where 
        except value = (L.filter ((/=) value) tries)
        endWord = if xs == [] then True else False

allWords :: [Trie b] -> [[b]]
allWords tries = 
    let raw = rawWords tries
    in map (flatMap id) raw
    where 
        flatMap f = Fold.concatMap (Fold.toList . f)
        rawWords tries = [key:next
                            | (Node key suffixes isWord) <- tries
                            , next <- 
                                if isWord then 
                                    []:(rawWords suffixes)
                                else 
                                    rawWords suffixes]