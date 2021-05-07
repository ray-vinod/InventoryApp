select * from Issues;
delete from Issues;
select * from SaleReturns;
delete from SaleReturns;
select * from Receives;
update Receives
set UseQuantity=0;
select * from Stocks;
update Stocks
set TotalIssue=0,TotalIssueReturn=0,InStock=15;

