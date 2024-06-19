using Carbunql;
using Carbunql.Building;
using Carbunql.TypeSafe;

internal class Program
{
    private static void Main(string[] args)
    {
        //選択クエリを取得する
        var query = SelectAllStore();


        // 選択クエリをコマンド文字列として出力する
        // select * from store
        Console.WriteLine(query.ToText());
        Console.WriteLine(";");

        //サブクエリとして再利用
        query = SelectStoreId1();
        Console.WriteLine(query.ToText());
        Console.WriteLine(";");

        //CTEとして再利用
        query = SelectStoreId1UseCTE();
        Console.WriteLine(query.ToText());
        Console.WriteLine(";");

        //CTEとして再利用＋テストデータ注入
        query = SelectStoreId1UseCTE();
        var store = SelectTestStoreData();
        //WITH句に注入
        query.With(() => store);
        Console.WriteLine(query.ToText());
        Console.WriteLine(";");

        //テンポラリテーブル化
        Console.WriteLine(query.ToCreateTableQuery("tmp", isTemporary: true).ToText());
        Console.WriteLine(";");
        //追加クエリ
        Console.WriteLine(query.ToInsertQuery("target_table").ToText());
        Console.WriteLine(";");
    }

    /// <summary>
    /// すべての店舗を取得する選択クエリ
    /// </summary>
    /// <returns></returns>
    private static FluentSelectQuery<store> SelectAllStore()
    {
        //データセット（テーブル）にエイリアス名をつける
        var s = Sql.DefineDataSet<store>();
        //Fromにデータセットを設定する
        return Sql.From(() => s);
    }

    /// <summary>
    /// 店舗ID=1を選択するクエリ（サブクエリ）
    /// </summary>
    /// <returns></returns>
    private static FluentSelectQuery<store> SelectStoreId1()
    {
        //データセット（クエリ）にエイリアス名をつける
        var all_s = Sql.DefineDataSet(() => SelectAllStore());
        //Fromにデータセットを設定する
        //Were条件も追加する
        return Sql.From(() => all_s)
            .Where(() => all_s.store_id == 1);
    }

    /// <summary>
    /// 店舗ID=1を選択するクエリ（CTE）
    /// </summary>
    /// <returns></returns>
    private static FluentSelectQuery<store> SelectStoreId1UseCTE()
    {
        //選択クエリに名前をつけるとCTEと認識します
        var all_store = SelectAllStore();

        //データセット（CTE）にエイリアス名をつける
        var all_s = Sql.DefineDataSet(() => all_store);
        //Fromにデータセットを設定する
        //Were条件も追加する
        return Sql.From(() => all_s)
            .Where(() => all_s.store_id == 1);
    }

    private static FluentSelectQuery<store> SelectTestStoreData()
    {
        //コレクションからVALUESクエリを生成し、さらに選択クエリにする
        return new FluentSelectQuery<store>([
               new store(){store_id = 1, store_name = "abc"},
               new store(){store_id = 2, store_name = "def"},
        ]).Comment($"Function:{nameof(SelectTestStoreData)}");
    }
}

public record store : IDataRow
{
    public long store_id { get; set; }
    public string store_name { get; set; }
    public IDataSet DataSet { get; set; } = null!;
}

