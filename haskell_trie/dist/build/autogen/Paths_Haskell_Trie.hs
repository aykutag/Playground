module Paths_Haskell_Trie (
    version,
    getBinDir, getLibDir, getDataDir, getLibexecDir,
    getDataFileName
  ) where

import qualified Control.Exception as Exception
import Data.Version (Version(..))
import System.Environment (getEnv)
import Prelude

catchIO :: IO a -> (Exception.IOException -> IO a) -> IO a
catchIO = Exception.catch


version :: Version
version = Version {versionBranch = [0,1,0,0], versionTags = []}
bindir, libdir, datadir, libexecdir :: FilePath

bindir     = "/Users/akropp/Library/Haskell/ghc-7.6.3/lib/Haskell-Trie-0.1.0.0/bin"
libdir     = "/Users/akropp/Library/Haskell/ghc-7.6.3/lib/Haskell-Trie-0.1.0.0/lib"
datadir    = "/Users/akropp/Library/Haskell/ghc-7.6.3/lib/Haskell-Trie-0.1.0.0/share"
libexecdir = "/Users/akropp/Library/Haskell/ghc-7.6.3/lib/Haskell-Trie-0.1.0.0/libexec"

getBinDir, getLibDir, getDataDir, getLibexecDir :: IO FilePath
getBinDir = catchIO (getEnv "Haskell_Trie_bindir") (\_ -> return bindir)
getLibDir = catchIO (getEnv "Haskell_Trie_libdir") (\_ -> return libdir)
getDataDir = catchIO (getEnv "Haskell_Trie_datadir") (\_ -> return datadir)
getLibexecDir = catchIO (getEnv "Haskell_Trie_libexecdir") (\_ -> return libexecdir)

getDataFileName :: FilePath -> IO FilePath
getDataFileName name = do
  dir <- getDataDir
  return (dir ++ "/" ++ name)
