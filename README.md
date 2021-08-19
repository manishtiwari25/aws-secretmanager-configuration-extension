

This repository contains a provider for [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration/) that retrieves secrets stored in [AWS Secrets Manager](https://aws.amazon.com/secrets-manager/) using Secret Manger [Caching](https://docs.aws.amazon.com/secretsmanager/latest/userguide/use-client-side-caching.html).

## Cloning

```sh
git clone https://github.com/manishtiwari25/aws-secretmanager-configuration-extension.git
```

## Known issues
#### AWSSDK.Core SDK conflict issue 

this compile error you may face if you are using some other aws nugets, simple workaround is just install ASWSDK.Core nuget package in project.

This issue is caused because the AWS SDK has strict version boundaries forcing the usage of packages within the same major version family (e.g. you can't mix the AWS S3 3.5 package with AWS EC2 3.3 package).

https://github.com/aws/aws-sdk-net/issues/1846


