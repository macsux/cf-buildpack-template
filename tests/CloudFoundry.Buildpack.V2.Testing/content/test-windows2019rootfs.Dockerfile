FROM cloudfoundry/windows2016fs
RUN netsh http add urlacl url=http://*:8080/ user=Users
USER vcap
CMD powershell