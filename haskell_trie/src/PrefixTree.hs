module PrefixTree where

import qualified Data.List as L
import Data.Maybe
import Control.Monad
import qualified Data.Foldable as Fold

type Key a = [a]

data Trie key = Node (Maybe key) [Trie key] Bool deriving (Show, Eq, Read)

empty :: [Trie key]
empty = []

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
                
insert :: (Eq t) => Key t -> [Trie t] -> [Trie t]
insert [] t = t
insert (x:xs) tries = 
    case findKey x tries of 
        Nothing -> [(Node (Just x) (insert xs [])) isEndWord]++tries
        Just value -> 
            let (Node key next word) = value
            in [Node key (insert xs next) (toggleWordEnd word)]++(except value)
    where 
        except value = (L.filter ((/=) value) tries)
        isEndWord = if xs == [] then True else False
        toggleWordEnd old = if xs == [] then True else old

countChars :: [Trie t] -> Integer
countChars trie = count trie 0
    where 
        count [] num = num
        count ((Node _ next _):xs) num = 
            count (xs++next) (num + 1)


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