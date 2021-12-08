# AccessPoint
The AccessPoint tool is a .net command line utility that performs two high level functions.

## Asset Upload
When run in the ingest mode, the tool monitors a directory for new files.
When a new file exists, a hash of the file is created and compared to an online directory of hashes to see if the media asset has already been ingested to the platform.

If it hasn't then the tool encrypts the asset and opens a set of ingest streams to Azure blob storage, uploading the file in small blocks.
Each block is checked within Azure and as soon as blocks start to arrive they are made available to cinemas to begin streaming.
This means there is no delay from when an asset starts to be ingested to being available to retrieve. This is impoprtant since the turn around time from the end of the editing process to film release can be very short, particularly challenging for cinemas that have slow Internet connections. Also, the editing process can continue right up to the premiere and so this approach of treating each asset individually and optimising the compression and streaming ensures that cinemas are able to retrieve assets as quickly as possible.

* This tool is designed for Digital Cinema Packages (DCP) which is a collection of film assets that make up a film release. DCP files are the standard for Dolby cinemas and so support for DCP packages ensures that over 80% of cinemas are supported straight away.

* This version only supports Azure Blob storage for films since Azure cloud storage is the only storage environment approved by Disney and Marvel. However, th next major update will support more storage locations including Catalyst's Distributed File System (DFS) and the Interplanetary File System (IPFS). The process here is a little different in that the hashing process is part of DFS/IPFS which simplifies the checking process. However, the file ingest process will be slower due to not being able to open concurrent file streams in the way that blob storage allows. This is an area to be further investigated and optimised.

* Within blob storage, assets are copied to data centres around the world to create multiple regional download points for cinemas. It should be recognised that in this model there is one ingress to blob storage but potentially hundreds or even thousands of egress points to cinemas. This throws open economics issues since cloud providers tend to charge a low ingress fee but high egress fee. So while using cloud storage is highly efficient in terms of distribution times, it is also very expensive in terms of transfer costs. By contrast, DFS/IPFS is less efficient file transfer wise but is far more efficient economically. Striking the right balance will depend on the media content and further work on the DFS/IPFS model.

## Asset download
Cinemas using the same tool also monitor for new assets available to them and allows an equally efficient file download process. A cinema running the tool will start to download media assets as soon as the first blocks are uploaded and authorised for the cinema. This allows for a DCP package to be built up quickly, particularly important for premiere releases where a short turn around time is required.

## General notes
While this tool is designed with professional DCP packages in mind, transferred from film studios to cinemas around the world in a way that encrypts and secure content and optimises delivery, it can also be used for any type of media. The expection is to upgrade this tool to .NET 6 and to extend functionality considerably for home content producers and other types of producer.
