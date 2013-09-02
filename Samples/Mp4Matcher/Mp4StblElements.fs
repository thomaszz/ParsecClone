﻿namespace Mp4Matcher

open ParsecClone.BinaryCombinator
open ParsecClone.CombinatorBase
open System

[<AutoOpen>]
module Mp4StblElements =   
  
    let stts : VideoParser<_> = 
        atom "stts" >>= fun id ->
        versionAndFlags     >>= fun vFlags ->
        bp.uint32           >>= fun numEntries ->
        pStructs<TimeToSampleEntry> (int numEntries) >>= fun samples ->
        freeOpt >>. preturn {
            Atom = id
            VersionAndFlags = vFlags
            NumberOfEntries = numEntries
            SampleTimes = samples
        } |>> STTS

    let ctts : VideoParser<_> = 
        atom "ctts"         >>= fun id ->
        skipRemaining id.Size 8  >>= fun _ ->
        freeOpt >>. preturn id  |>> CTTS

    let stsd : VideoParser<_> = 
        atom "stsd"    >>= fun id ->
        versionAndFlags     >>= fun vFlags ->
        bp.uint32           >>= fun numEntries ->
        freeOpt >>. sampleDescription  |>> STSD
    
    let stsz : VideoParser<_> = 
        atom "stsz"    >>= fun id ->
        versionAndFlags     >>= fun vFlags ->
        bp.uint32           >>= fun sampleSize ->
        bp.uint32           >>= fun numEntries ->
        pStructs<SampleSizeEntry> (int numEntries) >>= fun samples ->
        freeOpt >>. preturn {
            Atom = id
            VersionAndFlags = vFlags
            NumberOfEntries = numEntries
            SampleSizes = samples
        } |>> STSZ


    let sampleToChunkEntry = 
        bp.uint32 >>= fun firstChunk ->
        bp.uint32 >>= fun samplesPerChunk ->
        bp.uint32 >>= fun sampleDescriptionId ->
        preturn {
            FirstChunk = firstChunk
            SamplesPerChunk = samplesPerChunk
            SampleDescriptionID = sampleDescriptionId
        }

    let stsc : VideoParser<_> = 
        atom "stsc" >>= fun id ->
        versionAndFlags     >>= fun vFlags ->
        bp.uint32           >>= fun numEntries ->
        exactly (int numEntries) sampleToChunkEntry >>= fun samples ->
        freeOpt >>. preturn {
            Atom = id
            VersionAndFlags = vFlags
            NumberOfEntries = numEntries
            SampleChunks = samples
        } |>> STSC

    let chunkOffSet = bp.uint32 >>= fun i -> preturn { ChunkOffset = i }

    let stco : VideoParser<_> = 
        atom "stco"    >>= fun id ->
        versionAndFlags     >>= fun vFlags ->
        bp.uint32           >>= fun numEntries ->
        exactly (int numEntries) chunkOffSet >>= fun offsets ->
        freeOpt >>. preturn {
            Atom = id
            VersionAndFlags = vFlags
            NumberOfEntries = numEntries
            ChunkOffsets = offsets
        } |>> STCO


    let stss : VideoParser<_> = 
        atom "stss" >>= fun id ->
        versionAndFlags     >>= fun vFlags ->
        bp.uint32           >>= fun numEntries ->
        pStructs<SyncSampleEntry> (int numEntries) >>= fun syncSamples ->
        freeOpt >>. preturn {
            Atom = id
            VersionAndFlags = vFlags
            NumberOfEntries = numEntries
            SyncSamples = syncSamples
        } |>> STSS
       
