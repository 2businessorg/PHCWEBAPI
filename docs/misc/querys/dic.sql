declare @tabela varchar(5) ='ft'
select distinct rtrim(tabela) tabela, nmtabela, cast(nomecampo as varchar(200)) nomecampo,tipo,comprimento,decimais,titulo
from dic
where tabela=@tabela
and nomecampo like @tabela+'%'
and nomecampo not like @tabela+'.u_%'
and nomecampo not like '%*-1'

