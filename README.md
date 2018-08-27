# SyncFilters
proof of concept for applying the syncfilters concept to dotmim.sync

The basic idea here is the following:
Dotmim.sync synchronizes sql tables incrementally. It does so by maintaingin a tracking table for each base table that is to be synchronized.
You can provide a filter to the sync logic, however, it will always be a constant expression. Whenever you want to change the sync filter (based on a new requirement for your domain logic), you would need to provision a new sync context and keep the old ones contact, as 
otherwise, this would break the sync process for all existing client applications out there.

The syncfilters approach solves this problem by introducing another table besides the tracking table: the syncfilters table.
For each user that shall receive a row from the base table, you need to add a syncfilter row. (and you can remove it anytime without affecting other users)

[Sync Filter Diagram](https://aeqhcq-ch3302.files.1drv.com/y4mL6PdPGoZZSZvTNDlzpk_MVZUna2enU8l3ZuHUbLb_1GAlZQPa-86ZXSPOtL-UgBOkK0UvnOttm28iaNW5tSi1asjd8ESfeA-duEaVBTHD1B_GvYke2r63Jr8ITtBtlpT9p1WImYUiglAsss9jeJHQ46RGdZ5HJWQvBtVPF2VqjNBaG6ayEstZEjdK3svFrpagvYZtR2184YaE2pkdI4aEw?width=1142&height=671&cropmode=none)
