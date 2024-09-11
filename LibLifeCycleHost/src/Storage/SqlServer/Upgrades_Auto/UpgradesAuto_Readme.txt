Auto upgrades automatically apply to existing deployments on host startup.

Auto upgrade scripts must:

* BE FAST! No index rebuilds or similar that can hang on a large table
* BE IDEMPOTENT! no-op if it ran before e.g. check if column already added before adding. Otherwise race condition possible if many nodes start concurrently.
* be backward compatible
* only alter definition of base tables but not stored proc, types etc. This is to keep upgrades simpler, we can script only latest version of types and procs.
    * need to drop obsolete unused versions of stored procs & types sometimes
* be named in order they should be applied and never change the name
    * follow suggested YYYYMMDD_XX_name pattern
* not contain any undefined sql parameters

Upgrade scripts can:

* be ether one per schema or one per life cycle.  In latter case use ___LIFECYCLE_NAME___ placeholder like in other scripts
* be archived or even deleted if we sure that all important deployments were upgraded (what to do after open sourcing though?)

Need a different solution for slow upgrades or breaking schema changes.
